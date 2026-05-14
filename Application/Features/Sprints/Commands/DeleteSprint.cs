using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.Sprints.Commands;

public class DeleteSprint
{
    public class Command : IRequest<Result<Unit>>
    {
        public required string ProjectId { get; set; }
        public required string SprintId { get; set; }
    }

    public class Handler(
        ISprintsRepository sprintsRepository,
        IProjectHistoryRecorder historyRecorder,
        ITenantContext tenantContext,
        IProjectsRepository projectsRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<Unit>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            if (!await projectsRepository.IsProjectExists(tenantId, request.ProjectId, cancellationToken))
                return Result<Unit>.Failure("Project not found.", HttpStatusCode.NotFound);

            var sprint = await sprintsRepository.GetByIdAsync(tenantId, request.SprintId, request.ProjectId, cancellationToken);
            if (sprint is null)
                return Result<Unit>.Failure("Sprint not found.", HttpStatusCode.NotFound);

            var hasItems = await sprintsRepository.SprintHasWorkItemsAsync(tenantId, request.SprintId, cancellationToken);
            if (hasItems)
                return Result<Unit>.Failure("Cannot delete a sprint that has work items.", HttpStatusCode.BadRequest);

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                await sprintsRepository.RemoveAsync(tenantId, request.SprintId, ct);
                historyRecorder.RecordDeletionChanges<Sprint>(sprint.ProjectId, sprint.Id, sprint.Name ?? sprint.Code, sprint.Code);
            }, cancellationToken);

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}
