using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Enums;
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
                await workItemsRepository.AddWorkItemsToSprintAsync(tenantId, request.ProjectId, request.SprintId, request.WorkItemIds, cancellationToken);

            }, cancellationToken);

            return Result<string>.Success("WorkItems added to sprint successfully.", HttpStatusCode.OK);
        }
    }
}
