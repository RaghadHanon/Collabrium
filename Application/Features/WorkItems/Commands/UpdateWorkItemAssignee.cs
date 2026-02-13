using Application.Common;
using Application.Features.WorkItems.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.WorkItems.Commands;

public sealed class UpdateWorkItemAssignee
{
    public sealed class Command : IRequest<Result<WorkItemDto>>
    {
        public required string ProjectId { get; set; }
        public required string WorkItemId { get; set; }
        public string? AssignedToUserId { get; set; }
    }

    public sealed class Handler(
        IWorkItemsRepository workItemsRepository,
        IWorkItemsReadModel workItemsReadModel,
        ITenantContext tenantContext,
        ICurrentUserAccessor currentUserAccessor,
        IUsersReadModel usersReadModel,
        IProjectsRepository projectsRepository,
        IUnitOfWork unitOfWork)
        : IRequestHandler<Command, Result<WorkItemDto>>
    {
        public async Task<Result<WorkItemDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<WorkItemDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            if (!await projectsRepository.IsProjectExists(tenantId, request.ProjectId, cancellationToken))
                return Result<WorkItemDto>.Failure("Project not found.", HttpStatusCode.NotFound);

            var currentUserId = currentUserAccessor.GetUserId();
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Result<WorkItemDto>.Failure("Current user not found.", HttpStatusCode.Unauthorized);

            var workItem = await workItemsRepository.GetByIdAsync(
                tenantId,
                request.ProjectId,
                request.WorkItemId,
                includeChildren: false,
                cancellationToken);

            if (workItem is null)
                return Result<WorkItemDto>.Failure("Work item not found in this project.", HttpStatusCode.NotFound);

            var decision = PatchHelpers.DecideStringPatch(request.AssignedToUserId);

            if (decision.Action == PatchHelpers.PatchStringAction.Ignore)
            {
                var currentDto = await workItemsReadModel.GetByIdAsync(tenantId, request.ProjectId, workItem.Id, cancellationToken);
                return currentDto is null
                    ? Result<WorkItemDto>.Failure("Work item loaded but could not be returned.", HttpStatusCode.InternalServerError)
                    : Result<WorkItemDto>.Success(currentDto, HttpStatusCode.OK);
            }

            var role = await currentUserAccessor.GetUserRole(currentUserId, cancellationToken);
            var isManager = role is not null && Roles.ProjectsManagerRoles.Contains(role);

            if ((workItem.Type == WorkItemType.Epic || workItem.Type == WorkItemType.UserStory)
                && !isManager)
                return Result<WorkItemDto>.Failure("Only admins can change the assignee of Epics and User Stories.", HttpStatusCode.Forbidden);

            var isWorkItemUnassigned = string.IsNullOrWhiteSpace(workItem.AssignedToUserId);
            var isCurrentAssignee = string.Equals(workItem.AssignedToUserId, currentUserId, StringComparison.Ordinal);

            if (decision.Action == PatchHelpers.PatchStringAction.Set)
            {
                var targetUserId = decision.Value!;

                var userExists = await usersReadModel.UserExistsAsync(targetUserId, cancellationToken);
                if (!userExists)
                    return Result<WorkItemDto>.Failure("Assigned user not found.", HttpStatusCode.BadRequest);

                var isUserToAssignHasAccess = await projectsRepository.IsUserHasAccessToProjectAsync(
                    tenantId, targetUserId, request.ProjectId, cancellationToken);

                if (!isUserToAssignHasAccess)
                    return Result<WorkItemDto>.Failure("Assigned user does not have access to the project.", HttpStatusCode.BadRequest);

                var assigningToSelf = string.Equals(targetUserId, currentUserId, StringComparison.Ordinal);

                if (!CanSetAssignee(isManager, isWorkItemUnassigned, isCurrentAssignee, assigningToSelf))
                    return Result<WorkItemDto>.Failure(
                        "You can only reassign items assigned to you, or take unassigned items for yourself. Managers can assign/reassign any item.",
                        HttpStatusCode.Forbidden);
            }
            else if (decision.Action == PatchHelpers.PatchStringAction.Clear)
            {
                if (!CanClearAssignee(isManager, isCurrentAssignee, isWorkItemUnassigned))
                    return Result<WorkItemDto>.Failure(
                        "Only the current assignee (or managers) can unassign this work item.",
                        HttpStatusCode.Forbidden);
            }

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                if (decision.Action == PatchHelpers.PatchStringAction.Clear)
                    workItem.AssignTo(null);           
                else
                    workItem.AssignTo(decision.Value!);

                await workItemsRepository.UpdateAsync(workItem, ct);
            }, cancellationToken);

            var dto = await workItemsReadModel.GetByIdAsync(
                tenantId,
                request.ProjectId,
                workItem.Id,
                cancellationToken);

            return dto is null
                ? Result<WorkItemDto>.Failure("Work item updated but could not be loaded.", HttpStatusCode.InternalServerError)
                : Result<WorkItemDto>.Success(dto, HttpStatusCode.OK);
        }

        private static bool CanSetAssignee(bool isManager, bool isWorkItemUnassigned, bool isCurrentAssignee, bool assigningToSelf)
        {
            if (isManager) return true;

            if (isWorkItemUnassigned) return assigningToSelf;

            return isCurrentAssignee;                   
        }

        private static bool CanClearAssignee(bool isManager, bool isCurrentAssignee, bool isWorkItemUnassigned)
        {
            if (isManager) return true;

            if (isWorkItemUnassigned) return true; 

            return isCurrentAssignee;
        }
    }
}