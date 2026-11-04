using Application.Common;
using Application.Events;
using Application.Features.ProjectsHistories.DTOs;
using Application.Features.WorkItems.DTOs;
using Application.Features.WorkItems.DTOs.TypeSpecificDetails;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.WorkItems.Commands;

public partial class UpdateWorkItem
{
    public class Command : IRequest<Result<WorkItemDto>>
    {
        public required string ProjectId { get; set; }
        public required string WorkItemId { get; set; }
        public required WorkItemType Type { get; set; }

        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? AcceptanceCriteria { get; set; }
        public int? EffortPoints { get; set; }
        public WorkItemPriority? Priority { get; set; }

        public string? SprintId { get; set; }
        public bool? IsBlocked { get; set; }

        public IWorkItemUpdateDetails? Details { get; set; }
    }

    public class Handler(
        IWorkItemsRepository workItemsRepository,
        IWorkItemsReadModel workItemsReadModel,
        ITenantContext tenantContext,
        ICurrentUserAccessor currentUserAccessor,
        IProjectsRepository projectsRepository,
        ISprintsRepository sprintsRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator
    ) : IRequestHandler<Command, Result<WorkItemDto>>
    {
        public async Task<Result<WorkItemDto>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var tenantId =
                tenantContext.CurrentTenant?.Id
                ?? tenantContext.TenantId;

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return Result<WorkItemDto>.Failure(
                    "Tenant not found.",
                    HttpStatusCode.NotFound);
            }

            var project = await projectsRepository.GetByIdAsync(
                tenantId,
                request.ProjectId,
                cancellationToken);

            if (project is null)
            {
                return Result<WorkItemDto>.Failure(
                    "Project not found.",
                    HttpStatusCode.NotFound);
            }

            var currentUser =
                await currentUserAccessor.GetCurrentUserAsync();

            if (currentUser is null)
            {
                return Result<WorkItemDto>.Failure(
                    "Current user not found.",
                    HttpStatusCode.Unauthorized);
            }

            var currentUserId = currentUser.Id;

            var workItem = await workItemsRepository.GetByIdAsync(
                tenantId,
                request.ProjectId,
                request.WorkItemId,
                includeChildren: true,
                cancellationToken);

            if (workItem is null)
            {
                return Result<WorkItemDto>.Failure(
                    "Work item not found in this project.",
                    HttpStatusCode.NotFound);
            }

            if (request.Type != workItem.Type)
            {
                return Result<WorkItemDto>.Failure(
                    "Work item type mismatch.",
                    HttpStatusCode.BadRequest);
            }

            var authorizationResult =
                await ValidateUpdatePermissionAsync(
                    workItem,
                    currentUserId,
                    cancellationToken);

            if (authorizationResult is not null)
            {
                return authorizationResult;
            }

            var sprintDecision =
                PatchHelpers.DecideStringPatch(
                    request.SprintId);

            var isSprintPatchRequested =
                sprintDecision.Action is
                    PatchHelpers.PatchStringAction.Set or
                    PatchHelpers.PatchStringAction.Clear;

            var targetSprintId = ResolveTargetSprintId(
                workItem.SprintId,
                sprintDecision.Action,
                sprintDecision.Value);

            var sprintValidationResult =
                await ValidateTargetSprintAsync(
                    tenantId,
                    request.ProjectId,
                    sprintDecision.Action,
                    targetSprintId,
                    cancellationToken);

            if (sprintValidationResult is not null)
            {
                return sprintValidationResult;
            }

            IReadOnlyList<WorkItem> userStoryChildren = [];

            if (isSprintPatchRequested)
            {
                var hierarchyValidationResult =
                    await ValidateChildParentSprintAsync(
                        tenantId,
                        request.ProjectId,
                        workItem,
                        targetSprintId,
                        cancellationToken);

                if (hierarchyValidationResult is not null)
                {
                    return hierarchyValidationResult;
                }

                userStoryChildren =
                    await LoadUserStoryChildrenAsync(
                        tenantId,
                        request.ProjectId,
                        workItem,
                        cancellationToken);
            }

            var titleDecision =
                PatchHelpers.DecideStringPatch(
                    request.Title);

            var oldWorkItemState =
                workItem.Clone();

            IReadOnlyList<RecordedProjectHistoryChangeDto>
                recordedChanges = [];

            await unitOfWork.ExecuteInTransactionAsync(
                async ct =>
                {
                    ApplyGeneralDetailsPatch(
                        workItem,
                        request,
                        titleDecision);

                    if (request.Priority.HasValue)
                    {
                        workItem.SetPriority(
                            request.Priority.Value);
                    }

                    if (isSprintPatchRequested)
                    {
                        await MoveHierarchyToSprintAsync(
                            workItem,
                            userStoryChildren,
                            targetSprintId,
                            ct);
                    }

                    ApplyBlockedPatch(
                        workItem,
                        request.IsBlocked);

                    var sprint = await LoadCurrentSprintAsync(
                        tenantId,
                        request.ProjectId,
                        workItem.SprintId,
                        ct);

                    ApplyTypeSpecificPatch(
                        workItem,
                        request,
                        project,
                        sprint);

                    await workItemsRepository.UpdateAsync(
                        workItem,
                        ct);

                    if (recordedChanges.Count > 0 &&
                        !string.IsNullOrWhiteSpace(
                            workItem.AssignedToUserId) &&
                        !string.Equals(
                            workItem.AssignedToUserId,
                            currentUserId,
                            StringComparison.Ordinal))
                    {
                        await mediator.Publish(
                            new WorkItemDetailsChangedEvent(
                                TenantId: tenantId,
                                ProjectId: workItem.ProjectId,
                                WorkItemId: workItem.Id,
                                WorkItemCode: workItem.Code,
                                WorkItemTitle: workItem.Title,
                                SprintId: workItem.SprintId,
                                AssignedToUserId:
                                    workItem.AssignedToUserId!,
                                ChangedByUserId: currentUserId,
                                ChangedByFullName:
                                    currentUser.FullName,
                                Changes: recordedChanges),
                            ct);
                    }
                },
                cancellationToken);

            var dto = await workItemsReadModel.GetByIdAsync(
                tenantId,
                request.ProjectId,
                workItem.Id,
                cancellationToken);

            return dto is null
                ? Result<WorkItemDto>.Failure(
                    "Work item updated but could not be loaded.",
                    HttpStatusCode.InternalServerError)
                : Result<WorkItemDto>.Success(
                    dto,
                    HttpStatusCode.OK);
        }

        private async Task<Result<WorkItemDto>?>
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

            return Result<WorkItemDto>.Failure(
                "You can only update items assigned to you. Managers can update any item.",
                HttpStatusCode.Forbidden);
        }

        private async Task<Result<WorkItemDto>?> ValidateTargetSprintAsync(
                string tenantId,
                string projectId,
                PatchHelpers.PatchStringAction sprintAction,
                string? targetSprintId,
                CancellationToken cancellationToken)
        {
            if (sprintAction !=
                PatchHelpers.PatchStringAction.Set)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(targetSprintId))
            {
                return Result<WorkItemDto>.Failure(
                    "Sprint id is required.",
                    HttpStatusCode.BadRequest);
            }

            var sprint = await sprintsRepository.GetByIdAsync(
                tenantId,
                targetSprintId,
                projectId,
                cancellationToken);

            if (sprint is null)
            {
                return Result<WorkItemDto>.Failure("Sprint not found in this project.", HttpStatusCode.BadRequest);
            }

            if (sprint.Status is SprintStatus.Cancelled or SprintStatus.Completed)
            {
                return Result<WorkItemDto>.Failure($"Can't assign work items to a {sprint.Status} sprint.", HttpStatusCode.BadRequest);
            }

            return null;
        }

        private async Task<Result<WorkItemDto>?> ValidateChildParentSprintAsync(
                string tenantId,
                string projectId,
                WorkItem workItem,
                string? targetSprintId,
                CancellationToken cancellationToken)
        {
            if (!WorkItemSprintHierarchyRules.IsUserStoryChildType(workItem.Type) 
                || string.IsNullOrWhiteSpace(workItem.ParentId))
                return null;

            var parent = await workItemsRepository.GetByIdAsync(
                tenantId,
                projectId,
                workItem.ParentId,
                includeChildren: false,
                cancellationToken);

            if (parent is null)
                return Result<WorkItemDto>.Failure("Parent work item was not found in this project.", HttpStatusCode.BadRequest);
            
            if (parent.Type != WorkItemType.UserStory)
                return null;

            var isSameSprint = WorkItemSprintHierarchyRules.AreInSameSprint(parent.SprintId, targetSprintId);

            if (isSameSprint)
                return null;

            return Result<WorkItemDto>.Failure("This work item must remain in the same sprint as its parent user story.", HttpStatusCode.BadRequest);
        }

        private async Task<IReadOnlyList<WorkItem>>
            LoadUserStoryChildrenAsync(
                string tenantId,
                string projectId,
                WorkItem workItem,
                CancellationToken cancellationToken)
        {
            if (workItem.Type != WorkItemType.UserStory)
            {
                return [];
            }

            var children =
                await workItemsRepository
                    .GetChildrenByParentIdAsync(
                        tenantId,
                        projectId,
                        workItem.Id,
                        cancellationToken);

            return children
                .Where(child =>
                    WorkItemSprintHierarchyRules
                        .IsUserStoryChildType(child.Type))
                .ToList();
        }

        private async Task MoveHierarchyToSprintAsync(
            WorkItem workItem,
            IReadOnlyList<WorkItem> userStoryChildren,
            string? targetSprintId,
            CancellationToken cancellationToken)
        {
            workItem.MoveToSprint(
                targetSprintId);

            foreach (var child in userStoryChildren)
            {
                child.MoveToSprint(
                    targetSprintId);

                await workItemsRepository.UpdateAsync(
                    child,
                    cancellationToken);
            }
        }

        private static string? ResolveTargetSprintId(
            string? currentSprintId,
            PatchHelpers.PatchStringAction action,
            string? requestedSprintId)
        {
            return action switch
            {
                PatchHelpers.PatchStringAction.Set =>
                    requestedSprintId,

                PatchHelpers.PatchStringAction.Clear =>
                    null,

                _ => currentSprintId
            };
        }

        private static void ApplyGeneralDetailsPatch(
            WorkItem workItem,
            Command request,
            PatchHelpers.PatchStringDecision titleDecision)
        {
            var patchedTitle =
                PatchHelpers.ApplyStringPatch(
                    workItem.Title,
                    request.Title);

            var newTitle =
                titleDecision.Action ==
                PatchHelpers.PatchStringAction.Clear
                    ? string.Empty
                    : patchedTitle ?? workItem.Title;

            var newDescription =
                PatchHelpers.ApplyStringPatch(
                    workItem.Description,
                    request.Description);

            var newAcceptanceCriteria =
                PatchHelpers.ApplyStringPatch(
                    workItem.AcceptanceCriteria,
                    request.AcceptanceCriteria);

            workItem.UpdateDetails(
                newTitle,
                newDescription,
                newAcceptanceCriteria,
                request.EffortPoints ??
                workItem.EffortPoints);
        }

        private static void ApplyBlockedPatch(
            WorkItem workItem,
            bool? isBlocked)
        {
            if (!isBlocked.HasValue)
            {
                return;
            }

            if (isBlocked.Value)
            {
                workItem.Block();
                return;
            }

            workItem.Unblock();
        }

        private async Task<Sprint?> LoadCurrentSprintAsync(
            string tenantId,
            string projectId,
            string? sprintId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(sprintId))
            {
                return null;
            }

            return await sprintsRepository.GetByIdAsync(
                tenantId,
                sprintId,
                projectId,
                cancellationToken);
        }

        private void ApplyTypeSpecificPatch(
            WorkItem workItem,
            Command request,
            Project project,
            Sprint? sprint)
        {
            if (request.Details is null)
            {
                return;
            }

            switch (workItem.Type)
            {
                case WorkItemType.UserStory:
                    ApplyUserStoryPatch(
                        (UserStory) workItem,
                        request.Details);
                    break;

                case WorkItemType.Bug:
                    ApplyBugPatch(
                        (Bug) workItem,
                        request.Details);
                    break;

                case WorkItemType.TestCase:
                    ApplyTestCasePatch(
                        (TestCase) workItem,
                        request.Details,
                        project,
                        sprint);
                    break;
            }
        }

        private static void ApplyUserStoryPatch(
            UserStory userStory,
            IWorkItemUpdateDetails details)
        {
            if (details is not
                UpdateUserStoryDetails updateDetails)
            {
                return;
            }

            if (updateDetails.StoryPoints.HasValue)
            {
                userStory.Estimate(
                    updateDetails.StoryPoints.Value);
            }
        }

        private static void ApplyBugPatch(
            Bug bug,
            IWorkItemUpdateDetails details)
        {
            if (details is not
                UpdateBugDetails updateDetails)
            {
                return;
            }

            if (updateDetails.StepsToReproduce is null &&
                updateDetails.ExpectedResult is null &&
                updateDetails.ActualResult is null)
            {
                return;
            }

            bug.UpdateRepro(
                updateDetails.StepsToReproduce ??
                bug.StepsToReproduce,
                updateDetails.ExpectedResult ??
                bug.ExpectedResult,
                updateDetails.ActualResult ??
                bug.ActualResult);
        }

        private void ApplyTestCasePatch(
            TestCase testCase,
            IWorkItemUpdateDetails details,
            Project project,
            Sprint? sprint)
        {
            if (details is not
                UpdateTestCaseDetails updateDetails)
            {
                return;
            }

            if (updateDetails.Preconditions is not null ||
                updateDetails.Steps is not null ||
                updateDetails.TestEnvironment.HasValue ||
                updateDetails.ExpectedResult is not null)
            {
                testCase.UpdateDefinition(
                    preconditions:
                        updateDetails.Preconditions ??
                        testCase.Preconditions,
                    steps:
                        updateDetails.Steps ??
                        testCase.Steps,
                    env:
                        updateDetails.TestEnvironment ??
                        testCase.TestEnvironment,
                    expectedResult:
                        updateDetails.ExpectedResult ??
                        testCase.ExpectedResult);
            }

            if (!updateDetails.Result.HasValue &&
                updateDetails.ActualResult is null)
            {
                return;
            }

            var executedBy =
                currentUserAccessor.GetUserId();

            if (string.IsNullOrWhiteSpace(executedBy))
            {
                throw new InvalidOperationException(
                    "User id not found.");
            }

            if (updateDetails.Result.HasValue &&
                updateDetails.Result.Value !=
                TestCaseResult.NotRun)
            {
                testCase.RecordOutcome(
                    updateDetails.Result.Value,
                    updateDetails.ActualResult,
                    executedBy,
                    project,
                    sprint);
            }
        }
    }
}