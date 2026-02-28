using Application.Common;
using Application.Features.WorkItems.DTOs.TypeSpecificDetails;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.WorkItems.Commands;

public sealed class CreateWorkItem
{
    public sealed class Command : IRequest<Result<string>>
    {
        public required string ProjectId { get; set; }
        public required string Title { get; set; }
        public required WorkItemType Type { get; set; }

        public string? Description { get; set; }
        public string? AcceptanceCriteria { get; set; }
        public int EffortPoints { get; set; }
        public WorkItemPriority Priority { get; set; } = WorkItemPriority.Medium;

        public IWorkItemCreateDetails? Details { get; set; }
    }

    public sealed class Handler(
        IWorkItemsRepository workItemsRepository,
        IProjectsRepository projectsRepository,
        IProjectHistoryRecorder historyRecorder,
        ITenantContext tenantContext,
        ICurrentUserAccessor currentUserAccessor,
        IUnitOfWork unitOfWork) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<string>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            if (!await projectsRepository.IsProjectExists(tenantId, request.ProjectId, cancellationToken))
                return Result<string>.Failure("Project not found.", HttpStatusCode.NotFound);

            var userId = currentUserAccessor.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Result<string>.Failure("User not found.", HttpStatusCode.Unauthorized);

            WorkItem? workItem = null;

            await unitOfWork.ExecuteInTransactionAsync(async cancellationToken =>
            {
                workItem = request.Type switch
                {
                    WorkItemType.Epic => new Epic(request.Title, userId!, request.ProjectId),

                    WorkItemType.TaskItem => new TaskItem(request.Title, userId!, request.ProjectId),

                    WorkItemType.UserStory when request.Details is CreateUserStoryDetails d
                        => new UserStory(request.Title, userId!, request.ProjectId, d.StoryPoints),

                    WorkItemType.Bug when request.Details is CreateBugDetails d
                        => new Bug(request.Title, userId!, request.ProjectId, d.ActualResult, d.ExpectedResult, d.StepsToReproduce),

                    WorkItemType.TestCase when request.Details is CreateTestCaseDetails d
                        => new TestCase(request.Title, userId!, request.ProjectId,
                            d.Preconditions, d.Steps, d.TestEnvironment, d.ExpectedResult),

                    WorkItemType.UserStory => throw new InvalidOperationException("UserStory details are missing or invalid."),
                    WorkItemType.Bug => throw new InvalidOperationException("Bug details are missing or invalid."),
                    WorkItemType.TestCase => throw new InvalidOperationException("TestCase details are missing or invalid."),

                    _ => throw new InvalidOperationException("Unsupported work workItem type.")
                };

                workItem.UpdateDetails(request.Title, request.Description, request.AcceptanceCriteria, request.EffortPoints);
                workItem.SetPriority(request.Priority);

                workItemsRepository.Add(workItem, cancellationToken);

                historyRecorder.RecordCreationChanges<WorkItem>(workItem.ProjectId, workItem.Id, workItem.Title);
            }, cancellationToken);

            return workItem is null
                ? Result<string>.Failure("Work workItem could not be created.", HttpStatusCode.InternalServerError)
                : Result<string>.Success(workItem.Id, HttpStatusCode.Created);
        }
    }
}