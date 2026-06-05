using Application.Common;
using Application.Events;
using Application.Features.Users.DTOs;
using Application.Features.WorkItems.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Models;
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
        IProjectHistoryRecorder historyRecorder,
        ITenantContext tenantContext,
        ICurrentUserAccessor currentUserAccessor,
        IUsersReadModel usersReadModel,
        IProjectsRepository projectsRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator)
        : IRequestHandler<Command, Result<WorkItemDto>>
    {
        public async Task<Result<WorkItemDto>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id ?? tenantContext.TenantId;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<WorkItemDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            if (!await projectsRepository.IsProjectExists(tenantId, request.ProjectId, cancellationToken))
                return Result<WorkItemDto>.Failure("Project not found.", HttpStatusCode.NotFound);

            var currentUser = await currentUserAccessor.GetCurrentUserAsync(cancellationToken);
            if (currentUser is null)
                return Result<WorkItemDto>.Failure("Current user not found.", HttpStatusCode.Unauthorized);

            var workItem = await workItemsRepository.GetByIdAsync(
                tenantId,
                request.ProjectId,
                request.WorkItemId,
                includeChildren: false,
                cancellationToken);

            if (workItem is null)
                return Result<WorkItemDto>.Failure("Work item not found in this project.", HttpStatusCode.NotFound);

            var assigneePatch = PatchHelpers.DecideStringPatch(request.AssignedToUserId);

            if (assigneePatch.Action == PatchHelpers.PatchStringAction.Ignore)
            {
                var currentDto = await workItemsReadModel.GetByIdAsync(
                    tenantId,
                    request.ProjectId,
                    workItem.Id,
                    cancellationToken);

                return currentDto is null
                    ? Result<WorkItemDto>.Failure("Work item loaded but could not be returned.", HttpStatusCode.InternalServerError)
                    : Result<WorkItemDto>.Success(currentDto, HttpStatusCode.OK);
            }

            var role = await currentUserAccessor.GetUserRole(currentUser.Id, cancellationToken);

            var isManager = role is not null && Roles.ProjectsManagerRoles.Contains(role);
            var isWorkItemUnassigned = string.IsNullOrWhiteSpace(workItem.AssignedToUserId);
            var isCurrentAssignee = string.Equals(workItem.AssignedToUserId, currentUser.Id, StringComparison.Ordinal);
            var isCreator = string.Equals(workItem.CreatedByUserId, currentUser.Id, StringComparison.Ordinal);

            UserDto? assigneeUser = null;
            UserDto? previousAssigneeUser = null;

            if (assigneePatch.Action == PatchHelpers.PatchStringAction.Set)
            {
                var targetUserId = assigneePatch.Value!;

                assigneeUser = await usersReadModel.GetUserByIdAsync(targetUserId, cancellationToken);
                if (assigneeUser is null)
                    return Result<WorkItemDto>.Failure("Assigned user not found.", HttpStatusCode.BadRequest);

                var isUserToAssignHasAccess = await projectsRepository.IsUserHasAccessToProjectAsync(
                    tenantId,
                    targetUserId,
                    request.ProjectId,
                    cancellationToken);

                if (!isUserToAssignHasAccess)
                    return Result<WorkItemDto>.Failure(
                        "Assigned user does not have access to the project.",
                        HttpStatusCode.BadRequest);

                var assigningToSelf = string.Equals(targetUserId, currentUser.Id, StringComparison.Ordinal);

                if (!CanSetAssignee(isManager, isWorkItemUnassigned, isCreator, assigningToSelf))
                    return Result<WorkItemDto>.Failure(
                        "You can only take unassigned items for yourself. Managers and workItem creator can assign/reassign any item.",
                        HttpStatusCode.Forbidden);
            }
            else if (assigneePatch.Action == PatchHelpers.PatchStringAction.Clear)
            {
                if (!CanClearAssignee(isManager, isCurrentAssignee, isCreator))
                    return Result<WorkItemDto>.Failure(
                        "Only the current assignee, workItem creator or managers can unassign this work item.",
                        HttpStatusCode.Forbidden);

                if (!string.IsNullOrWhiteSpace(workItem.AssignedToUserId))
                {
                    previousAssigneeUser = await usersReadModel.GetUserByIdAsync(
                        workItem.AssignedToUserId,
                        cancellationToken);
                }
            }

            var shouldPublishAssignedEvent =
                assigneePatch.Action == PatchHelpers.PatchStringAction.Set &&
                assigneeUser is not null;

            var shouldPublishUnassignedEvent =
                assigneePatch.Action == PatchHelpers.PatchStringAction.Clear &&
                previousAssigneeUser is not null;

            var oldWorkItemState = workItem.Clone();

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                if (assigneePatch.Action == PatchHelpers.PatchStringAction.Clear)
                {
                    workItem.AssignTo(null);
                }
                else if (assigneePatch.Action == PatchHelpers.PatchStringAction.Set)
                {
                    workItem.AssignTo(assigneePatch.Value!);
                }

                await workItemsRepository.UpdateAsync(workItem, ct);

                historyRecorder.RecordUpdatingChanges<WorkItem>(
                    workItem.ProjectId,
                    workItem.Id,
                    workItem.Title,
                    workItem.Code,
                    oldWorkItemState,
                    workItem);

            }, cancellationToken);

            if (shouldPublishAssignedEvent)
            {
                await mediator.Publish(new WorkItemAssignedEvent(
                    tenantId,
                    request.ProjectId,
                    workItem.Id,
                    workItem.Title,
                    workItem.Code,
                    workItem.Type.ToString(),
                    workItem.Priority.ToString(),
                    workItem.SprintId,
                    assigneeUser!.Id,
                    assigneeUser.Email,
                    assigneeUser.FullName,
                    currentUser.Id,
                    currentUser.FullName
                ), cancellationToken);
            }

            if (shouldPublishUnassignedEvent)
            {
                await mediator.Publish(new WorkItemUnassignedEvent(
                    tenantId,
                    request.ProjectId,
                    workItem.Id,
                    workItem.Title,
                    workItem.Code,
                    workItem.Type.ToString(),
                    workItem.Priority.ToString(),
                    previousAssigneeUser!.Id,
                    previousAssigneeUser.Email,
                    previousAssigneeUser.FullName,
                    currentUser.Id,
                    currentUser.FullName
                ), cancellationToken);
            }

            var dto = await workItemsReadModel.GetByIdAsync(
                tenantId,
                request.ProjectId,
                workItem.Id,
                cancellationToken);

            return dto is null
                ? Result<WorkItemDto>.Failure("Work item updated but could not be loaded.", HttpStatusCode.InternalServerError)
                : Result<WorkItemDto>.Success(dto, HttpStatusCode.OK);
        }

        private static bool CanSetAssignee(
            bool isManager,
            bool isWorkItemUnassigned,
            bool isCreator,
            bool assigningToSelf)
        {
            return isManager ? true : isCreator ? true : isWorkItemUnassigned ? assigningToSelf : false;
        }

        private static bool CanClearAssignee(
            bool isManager,
            bool isCurrentAssignee,
            bool isCreator)
        {
            return isManager ? true : isCreator ? true : isCurrentAssignee;
        }
    }
}