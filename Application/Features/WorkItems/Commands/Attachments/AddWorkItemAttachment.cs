using Application.Common;
using Application.Features.WorkItems.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace Application.Features.WorkItems.Commands.Attachments;

public class AddWorkItemAttachment
{
    public class Command : IRequest<Result<Unit>>
    {
        public required string ProjectId { get; set; }
        public required string WorkItemId { get; set; }

        public required AttachmentType Type { get; set; }
        public required IFormFile File { get; set; }
    }

    public sealed class Handler(
        IAttachmentStorageFactory storageFactory,
        ITenantContext tenantContext,
        IWorkItemsRepository workItemsRepository,
        IProjectsRepository projectsRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserAccessor currentUserAccessor
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<Unit>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            if (!await projectsRepository.IsProjectExists(tenantId, request.ProjectId, cancellationToken))
                return Result<Unit>.Failure("Project not found.", HttpStatusCode.NotFound);

            var workItem = await workItemsRepository.GetByIdAsync(tenantId, request.ProjectId, request.WorkItemId, false, cancellationToken);
            if (workItem is null)
                return Result<Unit>.Failure("Source Work item not found in this project.", HttpStatusCode.NotFound);

            var currentUserId = currentUserAccessor.GetUserId();
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Result<Unit>.Failure("Current User not found", HttpStatusCode.NotFound);

            if (request.File is null || request.File.Length == 0)
                return Result<Unit>.Failure("No file uploaded.", HttpStatusCode.BadRequest);

            var isCurrentAssignee = string.Equals(workItem.AssignedToUserId, currentUserId, StringComparison.Ordinal);
            var userRole = await currentUserAccessor.GetUserRole(currentUserAccessor.GetUserId()!, cancellationToken);
            var isManager = Roles.ProjectsManagerRoles.Contains(userRole);

            if (!isManager && !isCurrentAssignee)
                return Result<Unit>.Failure(
                    "You can only update items assigned to you. Managers can update any item.",
                    HttpStatusCode.Forbidden);

            var storage = storageFactory.Get(request.Type);

            var stored = await storage.UploadAsync(
                request.File,
                request.Type,
                folder: "Collabrium/WorkItems/Attachments",
                ct: cancellationToken);

            if (stored is null)
                return Result<Unit>.Failure("Upload failed.", HttpStatusCode.BadRequest);

            await unitOfWork.ExecuteInTransactionAsync(async cancellationToken =>
            {
                var attachment = new Attachment(
                    fileName: stored.FileName,
                    storagePath: stored.StoragePath,
                    url: stored.Url,
                    contentType: stored.ContentType,
                    type: request.Type,
                    uploadedByUserId: currentUserId
                 );

                await workItemsRepository.AddAttachmentOnWorkItemAsync(
                    tenantId: tenantId,
                    projectId: request.ProjectId,
                    workItemId: request.WorkItemId,
                    attachment: attachment,
                    cancellationToken: cancellationToken);
            }, cancellationToken);

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}