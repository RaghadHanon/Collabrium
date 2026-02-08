using Application.Common;
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

public sealed partial class UpdateWorkItem
{
    public sealed class Command : IRequest<Result<WorkItemDto>>
    {
        public required string ProjectId { get; set; }
        public required string WorkItemId { get; set; }

        public required WorkItemType Type { get; set; }

        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? AcceptanceCriteria { get; set; }
        public int? EffortPoints { get; set; }
        public WorkItemPriority? Priority { get; set; }

        public string? AssignedToUserId { get; set; }
        public string? SprintId { get; set; }
        public bool? IsBlocked { get; set; }
        public WorkItemStatus? Status { get; set; }

        public IWorkItemUpdateDetails? Details { get; set; }
    }

    public sealed class Handler(
        IWorkItemsRepository workItemsRepository,
        IWorkItemsReadModel workItemsReadModel,
        ITenantContext tenantContext,
        ICurrentUserAccessor currentUserAccessor,
        IUsersReadModel usersReadModel,
        IProjectsRepository projectsRepository,
        ISprintsRepository sprintsRepository,
        IUnitOfWork unitOfWork)
        : IRequestHandler<Command, Result<WorkItemDto>>
    {
        public async Task<Result<WorkItemDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<WorkItemDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            if (request.ProjectId is not null && !(await projectsRepository.IsProjectExists(tenantId, request.ProjectId!, cancellationToken)))
                return Result<WorkItemDto>.Failure("Project not found.", HttpStatusCode.BadRequest);

            var workItem = await workItemsRepository.GetByIdAsync(
                tenantId,
                request.ProjectId,
                request.WorkItemId,
                includeChildren: true,
                cancellationToken);

            if (workItem is null)
                return Result<WorkItemDto>.Failure("Work item not found in this project.", HttpStatusCode.NotFound);

            if (request.SprintId is not null && !(await sprintsRepository.IsExistsAsync(tenantId, request.SprintId, cancellationToken)))
                return Result<WorkItemDto>.Failure("Sprint not found in this project.", HttpStatusCode.BadRequest);

            if (request.Type != workItem.Type)
                return Result<WorkItemDto>.Failure("Work item type mismatch.", HttpStatusCode.BadRequest);

            var isUserValidToBeAssigned = request.AssignedToUserId is not null
                 && await usersReadModel.UserExistsAsync(request.AssignedToUserId, cancellationToken)
                 && await projectsRepository.IsUserHasAccessToProjectAsync(tenantId, request.AssignedToUserId, request.ProjectId, cancellationToken);

            if (!isUserValidToBeAssigned)
                return Result<WorkItemDto>.Failure("Assigned user is invalid or does not have access to the project.", HttpStatusCode.BadRequest);

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                workItem.UpdateDetails(
                    request.Title ?? workItem.Title,
                    request.Description ?? workItem.Description,
                    request.AcceptanceCriteria ?? workItem.AcceptanceCriteria,
                    request.EffortPoints ?? workItem.EffortPoints
                   );

                if (request.Priority.HasValue)
                    workItem.SetPriority(request.Priority.Value);

                if (isUserValidToBeAssigned)
                    workItem.AssignTo(request.AssignedToUserId);

                if (request.SprintId is not null)
                    workItem.MoveToSprint(request.SprintId);

                if (request.IsBlocked.HasValue)
                {
                    if (request.IsBlocked.Value) workItem.Block();
                    else workItem.Unblock();
                }

                if (request.Status.HasValue)
                    workItem.ChangeStatus(request.Status.Value);

                ApplyTypeSpecificPatch(workItem, request);

                await workItemsRepository.UpdateAsync(workItem, ct);
            }, cancellationToken);

            var dto = await workItemsReadModel.GetByIdAsync(tenantId, request.ProjectId, workItem.Id, cancellationToken);

            return dto is null
                ? Result<WorkItemDto>.Failure("Work item updated but could not be loaded.", HttpStatusCode.InternalServerError)
                : Result<WorkItemDto>.Success(dto, HttpStatusCode.OK);
        }

        private void ApplyTypeSpecificPatch(WorkItem workItem, Command request)
        {
            if (request.Details is null) return;

            switch (workItem.Type)
            {
                case WorkItemType.UserStory:
                    ApplyUserStoryPatch((UserStory) workItem, request.Details);
                    break;

                case WorkItemType.Bug:
                    ApplyBugPatch((Bug) workItem, request.Details);
                    break;

                case WorkItemType.TestCase:
                    ApplyTestCasePatch((TestCase) workItem, request.Details);
                    break;

                default:
                    break;
            }
        }

        private static void ApplyUserStoryPatch(UserStory us, IWorkItemUpdateDetails details)
        {
            if (details is not UpdateUserStoryDetails d) return;
            if (d.StoryPoints.HasValue)
                us.Estimate(d.StoryPoints.Value);
        }

        private static void ApplyBugPatch(Bug bug, IWorkItemUpdateDetails details)
        {
            if (details is not UpdateBugDetails d) return;

            if (d.StepsToReproduce is null && d.ExpectedResult is null && d.ActualResult is null)
                return;

            bug.UpdateRepro(
                d.StepsToReproduce ?? bug.StepsToReproduce,
                d.ExpectedResult ?? bug.ExpectedResult,
                d.ActualResult ?? bug.ActualResult
               );
        }

        private void ApplyTestCasePatch(TestCase tc, IWorkItemUpdateDetails details)
        {
            if (details is not UpdateTestCaseDetails d) return;

            if (d.Preconditions is not null ||
                d.Steps is not null ||
                d.TestEnvironment.HasValue ||
                d.ExpectedResult is not null)
            {
                tc.UpdateDefinition(
                    preconditions: d.Preconditions ?? tc.Preconditions,
                    steps: d.Steps ?? tc.Steps,
                    env: d.TestEnvironment ?? tc.TestEnvironment,
                    expectedResult: d.ExpectedResult ?? tc.ExpectedResult);
            }

            if (d.Result.HasValue || d.ActualResult is not null)
            {
                var executedBy = currentUserAccessor.GetUserId();
                if (string.IsNullOrWhiteSpace(executedBy))
                    throw new InvalidOperationException("User id not found.");

                if (d.Result.HasValue && d.Result.Value != TestCaseResult.NotRun)
                {
                    tc.RecordOutcome(d.Result.Value, d.ActualResult, executedBy);
                }
            }
        }
    }
}