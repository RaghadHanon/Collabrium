using Application.Common;
using Application.Features.WorkItems.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.WorkItems.Commands.Relations;

public class AddWorkItemRelation
{
    public class Command : IRequest<Result<Unit>>
    {
        public required string ProjectId { get; set; }
        public required string SourceWorkItemId { get; set; }
        public required string TargetWorkItemId { get; set; }
        public required WorkItemRelationType RelationType { get; set; }
    }

    public class Handler(
        IWorkItemsRepository workItemsRepository,
        IProjectHistoryRecorder historyRecorder,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        IProjectsRepository projectsRepository,
        ICurrentUserAccessor currentUserAccessor
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return Result<Unit>.Failure("Tenant not found.", HttpStatusCode.NotFound);
            }

            var projectExists = await projectsRepository.IsProjectExists(
                tenantId,
                request.ProjectId,
                cancellationToken);

            if (!projectExists)
            {
                return Result<Unit>.Failure("Project not found.", HttpStatusCode.NotFound);
            }

            var currentUserId = currentUserAccessor.GetUserId();

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Result<Unit>.Failure("Current user not found.", HttpStatusCode.Unauthorized);
            }

            if (string.Equals(request.SourceWorkItemId, request.TargetWorkItemId, StringComparison.Ordinal))
            {
                return Result<Unit>.Failure("A work item cannot be related to itself.", HttpStatusCode.BadRequest);
            }

            var sourceWorkItem = await workItemsRepository.GetByIdAsync(
                tenantId,
                request.ProjectId,
                request.SourceWorkItemId,
                includeChildren: true,
                cancellationToken);

            if (sourceWorkItem is null)
            {
                return Result<Unit>.Failure("Source work item not found in this project.", HttpStatusCode.NotFound);
            }

            var targetWorkItem = await workItemsRepository.GetByIdAsync(
                tenantId,
                request.ProjectId,
                request.TargetWorkItemId,
                includeChildren: true,
                cancellationToken);

            if (targetWorkItem is null)
            {
                return Result<Unit>.Failure("Target work item not found in this project.", HttpStatusCode.NotFound);
            }

            var authorizationResult =
                await ValidateUpdatePermissionAsync(
                    sourceWorkItem,
                    currentUserId,
                    cancellationToken);

            if (authorizationResult is not null)
            {
                return authorizationResult;
            }

            var requiresSameSprint = WorkItemSprintHierarchyRules.RequiresSameSprint(
                    sourceWorkItem.Type,
                    targetWorkItem.Type,
                    request.RelationType);

            var areInSameSprint =
                WorkItemSprintHierarchyRules.AreInSameSprint(
                    sourceWorkItem.SprintId,
                    targetWorkItem.SprintId);

            if (requiresSameSprint &&
                !areInSameSprint)
            {
                return Result<Unit>.Failure("A user story and its child work items must be in the same sprint.", HttpStatusCode.BadRequest);
            }

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                await workItemsRepository.PersistRelationAsync(
                    tenantId,
                    request.ProjectId,
                    sourceWorkItem.Id,
                    targetWorkItem.Id,
                    request.RelationType,
                    ct);

                ApplyParentChildRelation(
                    sourceWorkItem,
                    targetWorkItem,
                    request.RelationType);

                await workItemsRepository.UpdateAsync(
                    sourceWorkItem,
                    ct);

                await workItemsRepository.UpdateAsync(
                    targetWorkItem,
                    ct);

                historyRecorder.RecordCreationChanges<WorkItemRelation>(
                    sourceWorkItem.ProjectId,
                    sourceWorkItem.Id,
                    sourceWorkItem.Title,
                    sourceWorkItem.Code);
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

            if (isManager ||
                isCurrentAssignee ||
                isCreator)
            {
                return null;
            }

            return Result<Unit>.Failure(
                "You can only update items assigned to you. Managers can update any item.",
                HttpStatusCode.Forbidden);
        }

        private static void ApplyParentChildRelation(
            WorkItem sourceWorkItem,
            WorkItem targetWorkItem,
            WorkItemRelationType relationType)
        {
            switch (relationType)
            {
                case WorkItemRelationType.Parent:
                    sourceWorkItem.SetParent(targetWorkItem.Id);
                    break;

                case WorkItemRelationType.Child:
                    targetWorkItem.SetParent(sourceWorkItem.Id);
                    break;
            }
        }
    }
}