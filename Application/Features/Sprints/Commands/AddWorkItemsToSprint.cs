using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Enums;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.Sprints.Commands;
public class AddWorkItemsToSprint
{
    public class Command : IRequest<Result<string>>
    {
        public required string ProjectId { get; set; }
        public required string SprintId { get; set; }
        public required string[] WorkItemIds { get; set; } = Array.Empty<string>();
    }

    public class Handler(
        ISprintsRepository sprintsRepository,
        IProjectHistoryRecorder historyRecorder,
        IWorkItemsRepository workItemsRepository,
        IProjectsRepository projectsRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork
        ) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<string>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var project = await projectsRepository.GetByIdAsync(tenantId, request.ProjectId, cancellationToken);
            if (project is null)
                return Result<string>.Failure("Project not found.", HttpStatusCode.NotFound);

            var sprint = await sprintsRepository.GetByIdAsync(
                   tenantId,
                   request.SprintId,
                   request.ProjectId,
                   cancellationToken);

            if (sprint is null)
                return Result<string>.Failure("Sprint not found in this project.", HttpStatusCode.BadRequest);

            if (sprint.Status == SprintStatus.Cancelled || sprint.Status == SprintStatus.Completed)
                return Result<string>.Failure($"Can't assign workItems to a {sprint.Status} sprint.", HttpStatusCode.BadRequest);

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var workItems = await workItemsRepository.GetWorkItemsListFromIds(tenantId, request.ProjectId, request.SprintId, request.WorkItemIds, cancellationToken);

                foreach (var workItem in workItems)
                {
                    var oldWorkItemState = workItem.Clone();
                    workItem.MoveToSprint(sprint.Id);
                    await workItemsRepository.UpdateAsync(workItem, cancellationToken);
                    await historyRecorder.RecordUpdatingChangesAsync<WorkItem>(workItem.ProjectId, workItem.Id, workItem.Title, workItem.Code, oldWorkItemState, workItem);
                }
            }, cancellationToken);

            return Result<string>.Success("WorkItems added to sprint successfully.", HttpStatusCode.OK);
        }
    }
}
