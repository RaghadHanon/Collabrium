using Application.Common;
using Application.Features.WorkItems.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.WorkItems.Commands;

public class UpdateWorkItemStatus
{
    public class Command : IRequest<Result<WorkItemDto>>
    {
        public required string ProjectId { get; set; }
        public required string WorkItemId { get; set; }
        public required WorkItemStatus NewStatus { get; set; }
    }

    public sealed class Handler(
        IWorkItemsRepository workItemsRepository,
        IProjectHistoryRecorder historyRecorder,
        ISprintsRepository sprintsRepository,
        IWorkItemsReadModel workItemsReadModel,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        ICurrentUserAccessor currentUserAccessor,
        IProjectsRepository projectsRepository
    ) : IRequestHandler<Command, Result<WorkItemDto>>
    {
        public async Task<Result<WorkItemDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<WorkItemDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var project = await projectsRepository.GetByIdAsync(tenantId, request.ProjectId, cancellationToken);
            if (project is null)
                return Result<WorkItemDto>.Failure("Project not found.", HttpStatusCode.NotFound);

            var currentUserId = currentUserAccessor.GetUserId();
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Result<WorkItemDto>.Failure("Current user not found.", HttpStatusCode.Unauthorized);

            var workItem = await workItemsRepository.GetByIdAsync(
                tenantId,
                request.ProjectId,
                request.WorkItemId,
                includeChildren: true,
                cancellationToken);

            if (workItem is null)
                return Result<WorkItemDto>.Failure("Work item not found in this project.", HttpStatusCode.NotFound);

            if (workItem.AssignedToUserId is null)
                return Result<WorkItemDto>.Failure("You can't change WoorkItem status without assigning it to someone.", HttpStatusCode.BadRequest);

            var userRole = await currentUserAccessor.GetUserRole(currentUserAccessor.GetUserId()!, cancellationToken);
            var isManager = Roles.ProjectsManagerRoles.Contains(userRole);

            var isCurrentAssignee = string.Equals(workItem.AssignedToUserId, currentUserId, StringComparison.Ordinal);

            if (!isManager && !isCurrentAssignee)
                return Result<WorkItemDto>.Failure(
                    "You can only update items assigned to you. Managers can update any item.",
                    HttpStatusCode.Forbidden);

            var allowedStatuses = workItem.GetAllowedStatuses();
            if (!allowedStatuses.Contains(request.NewStatus))
                return Result<WorkItemDto>.Failure($"Invalid status transition. Allowed statuses: {string.Join(", ", allowedStatuses)}.", HttpStatusCode.BadRequest);

            if (workItem.Type == WorkItemType.TestCase && ((TestCase) workItem).Result != TestCaseResult.Passed && request.NewStatus == WorkItemStatus.Done)
                return Result<WorkItemDto>.Failure("Test case must be marked as Passed before it can be marked as Done.", HttpStatusCode.BadRequest);

            var oldWorkItemState = workItem.Clone();
            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var children = await workItemsReadModel.GetChildrenAsync(tenantId, request.ProjectId, request.WorkItemId, ct);
                var sprint = await sprintsRepository.GetByIdAsync(tenantId, workItem.SprintId, request.ProjectId, ct);

                workItem.ChangeStatusWithValidation(request.NewStatus, project, sprint, children);

                await workItemsRepository.UpdateAsync(workItem, ct);

                historyRecorder.RecordUpdatingChanges<WorkItem>(workItem.ProjectId, workItem.Id, workItem.Title, workItem.Code, oldWorkItemState, workItem);
            }, cancellationToken);

            var dto = await workItemsReadModel.GetByIdAsync(
                tenantId,
                request.ProjectId,
                request.WorkItemId,
                cancellationToken);

            return dto is null
                ? Result<WorkItemDto>.Failure("Work item updated but could not be loaded.", HttpStatusCode.InternalServerError)
                : Result<WorkItemDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}