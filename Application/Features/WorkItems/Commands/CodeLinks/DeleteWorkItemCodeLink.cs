using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.WorkItems.Commands.CodeLinks;

public class DeleteWorkItemCodeLink
{
    public class Command : IRequest<Result<Unit>>
    {
        public required string ProjectId { get; set; }
        public required string WorkItemId { get; set; }
        public required string CodeLinkId { get; set; }
    }

    public class Handler(
        IWorkItemsRepository workItemsRepository,
        IProjectHistoryRecorder historyRecorder,
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
                return Result<Unit>.Failure("Work item not found in this project.", HttpStatusCode.NotFound);

            var currentUserId = currentUserAccessor.GetUserId();
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Result<Unit>.Failure("Current User not found.", HttpStatusCode.NotFound);

            var codeLink = await workItemsRepository.GetCodeLinkOnWorkItemByIdAsync(request.WorkItemId, request.CodeLinkId, cancellationToken);
            if (codeLink == null)
                return Result<Unit>.Failure("Code link not found.", HttpStatusCode.NotFound);

            var authorizationResult =
                await ValidateUpdatePermissionAsync(
                    workItem,
                    currentUserId,
                    cancellationToken);

            if (authorizationResult is not null)
            {
                return authorizationResult;
            }

            if (currentUserId != codeLink.CreatedByUserId)
                return Result<Unit>.Failure("You don't own this code link to delete it.", HttpStatusCode.Forbidden);

            await unitOfWork.ExecuteInTransactionAsync(async cancellationToken =>
            {
                await workItemsRepository.RemoveCodeLinkFromWorkItemAsync(
                    request.WorkItemId,
                    request.CodeLinkId,
                    cancellationToken);

                historyRecorder.RecordDeletionChanges<CodeLink>(workItem.ProjectId, workItem.Id, workItem.Title, workItem.Code);
            }, cancellationToken);

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }

        private async Task<Result<Unit>?>
            ValidateUpdatePermissionAsync(
                WorkItem workItem,
                string currentUserId,
                CancellationToken cancellationToken)
        {
            var isCurrentAssignee = string.Equals(
                workItem.AssignedToUserId,
                currentUserId,
                StringComparison.Ordinal);

            var isCreator = string.Equals(
                workItem.CreatedByUserId,
                currentUserId,
                StringComparison.Ordinal);

            var userRole =
                await currentUserAccessor.GetUserRole(
                    currentUserId,
                    cancellationToken);

            var isManager =
                Roles.ProjectsManagerRoles.Contains(
                    userRole);

            return isManager ||
                isCurrentAssignee ||
                isCreator
                ? null
                : Result<Unit>.Failure(
                "You can only update items assigned to you. Managers can update any item.",
                HttpStatusCode.Forbidden);
        }
    }
}