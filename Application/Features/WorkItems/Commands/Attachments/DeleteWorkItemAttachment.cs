using Application.Common;
using Application.Features.WorkItems.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.WorkItems.Commands.Attachments;

public class DeleteWorkItemAttachment
{
    public class Command : IRequest<Result<Unit>>
    {
        public required string ProjectId { get; set; }
        public required string WorkItemId { get; set; }
        public required string AttachmentId { get; set; }
    }

    public class Handler(
        IWorkItemsRepository workItemsRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        ICurrentUserAccessor currentUserAccessor,
        IProjectsRepository projectsRepository
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
                return Result<Unit>.Failure("Current User not found.", HttpStatusCode.NotFound);

            var codeLink = await workItemsRepository.GetAttachmentOnWorkItemByIdAsync(request.WorkItemId, request.AttachmentId, cancellationToken);
            if (codeLink == null)
                return Result<Unit>.Failure("Attachment not found.", HttpStatusCode.NotFound);

            var isCurrentAssignee = string.Equals(workItem.AssignedToUserId, currentUserId, StringComparison.Ordinal);
            var userRole = await currentUserAccessor.GetUserRole(currentUserAccessor.GetUserId()!, cancellationToken);
            var isManager = Roles.ProjectsManagerRoles.Contains(userRole);

            if (!isManager && !isCurrentAssignee)
                return Result<Unit>.Failure(
                    "You can only update items assigned to you. Managers can update any item.",
                    HttpStatusCode.Forbidden);

            if (currentUserId != codeLink.UploadedByUserId)
                return Result<Unit>.Failure("You don't own this attachment to delete it.", HttpStatusCode.Forbidden);

            await unitOfWork.ExecuteInTransactionAsync(async cancellationToken =>
            {
                await workItemsRepository.RemoveAttachmentFromWorkItemAsync(
                    request.WorkItemId,
                    request.AttachmentId,
                    cancellationToken);
            }, cancellationToken);

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}