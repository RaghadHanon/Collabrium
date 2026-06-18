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
        IProjectHistoryRecorder historyRecorder,
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

            var isCurrentAssignee = string.Equals(
                workItem.AssignedToUserId,
                currentUserId,
                StringComparison.Ordinal);

            var userRole = await currentUserAccessor.GetUserRole(
                currentUserId,
                cancellationToken);

            var isManager =
                Roles.ProjectsManagerRoles.Contains(userRole);

            if (!isManager && !isCurrentAssignee)
            {
                return Result<WorkItemDto>.Failure(
                    "You can only update items assigned to you. Managers can update any item.",
                    HttpStatusCode.Forbidden);
            }

            var sprintDecision =
                PatchHelpers.DecideStringPatch(request.SprintId);

            if (sprintDecision.Action ==
                PatchHelpers.PatchStringAction.Set)
            {
                var sprintIdToSet = sprintDecision.Value!;

                var sprintToSet =
                    await sprintsRepository.GetByIdAsync(
                        tenantId,
                        sprintIdToSet,
                        request.ProjectId,
                        cancellationToken);

                if (sprintToSet is null)
                {
                    return Result<WorkItemDto>.Failure(
                        "Sprint not found in this project.",
                        HttpStatusCode.BadRequest);
                }

                if (sprintToSet.Status is
                    SprintStatus.Cancelled or SprintStatus.Completed)
                {
                    return Result<WorkItemDto>.Failure(
                        $"Can't assign work items to a {sprintToSet.Status} sprint.",
                        HttpStatusCode.BadRequest);
                }
            }

            var titleDecision =
                PatchHelpers.DecideStringPatch(request.Title);

            var oldWorkItemState = workItem.Clone();

            IReadOnlyList<RecordedProjectHistoryChangeDto>
                recordedChanges = [];

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
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

                if (request.Priority.HasValue)
                {
                    workItem.SetPriority(
                        request.Priority.Value);
                }

                if (sprintDecision.Action ==
                    PatchHelpers.PatchStringAction.Clear)
                {
                    workItem.MoveToSprint(null);
                }
                else if (sprintDecision.Action ==
                         PatchHelpers.PatchStringAction.Set)
                {
                    workItem.MoveToSprint(
                        sprintDecision.Value);
                }

                if (request.IsBlocked.HasValue)
                {
                    if (request.IsBlocked.Value)
                    {
                        workItem.Block();
                    }
                    else
                    {
                        workItem.Unblock();
                    }
                }

                Sprint? sprint = null;

                if (!string.IsNullOrWhiteSpace(workItem.SprintId))
                {
                    sprint = await sprintsRepository.GetByIdAsync(
                        tenantId,
                        workItem.SprintId,
                        request.ProjectId,
                        ct);
                }

                ApplyTypeSpecificPatch(
                    workItem,
                    request,
                    project,
                    sprint);

                await workItemsRepository.UpdateAsync(
                    workItem,
                    ct);

                if (recordedChanges.Count > 0 &&
                    !string.IsNullOrWhiteSpace(workItem.AssignedToUserId) &&
                    !string.Equals(workItem.AssignedToUserId, currentUserId, StringComparison.Ordinal))
                {
                    await mediator.Publish(
                    new WorkItemDetailsChangedEvent(
                    TenantId: tenantId,
                    ProjectId: workItem.ProjectId,
                    WorkItemId: workItem.Id,
                    WorkItemCode: workItem.Code,
                    WorkItemTitle: workItem.Title,
                    SprintId: workItem.SprintId,
                    AssignedToUserId: workItem.AssignedToUserId!,
                    ChangedByUserId: currentUserId,
                    ChangedByFullName: currentUser.FullName,
                    Changes: recordedChanges
                    ),
                    cancellationToken);
                }
            }, cancellationToken);

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

        private void ApplyTypeSpecificPatch(
            WorkItem workItem,
            Command request,
            Project project,
            Sprint? sprint)
        {
            if (request.Details is null)
                return;

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
            if (details is not UpdateUserStoryDetails updateDetails)
                return;

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
            if (details is not UpdateBugDetails updateDetails)
                return;

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
            if (details is not UpdateTestCaseDetails updateDetails)
                return;

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

            if (updateDetails.Result.HasValue ||
                updateDetails.ActualResult is not null)
            {
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
}