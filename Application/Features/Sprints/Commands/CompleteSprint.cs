using Application.Common;
using Application.Features.Sprints.DTOs;
using Application.Features.WorkItems.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Sprints.Commands;

public class CompleteSprint
{
    public class Command : IRequest<Result<SprintDto>>
    {
        public required string ProjectId { get; set; }
        public required string SprintId { get; set; }
    }

    public class Handler(
        ISprintsRepository sprintsRepository,
        ISprintsReadModel sprintsReadModel,
        ITenantContext tenantContext,
        IProjectsRepository projectsRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<Command, Result<SprintDto>>
    {
        public async Task<Result<SprintDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<SprintDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            if (!(await projectsRepository.IsProjectExists(tenantId, request.ProjectId, cancellationToken)))
                return Result<SprintDto>.Failure("Project not found.", HttpStatusCode.NotFound);

            var sprint = await sprintsRepository.GetByIdAsync(tenantId, request.SprintId, request.ProjectId, cancellationToken);
            if (sprint is null)
                return Result<SprintDto>.Failure("Sprint not found.", HttpStatusCode.NotFound);

            await unitOfWork.ExecuteInTransactionAsync(async cancellationToken =>
            {
                sprint.Complete();
                await sprintsRepository.UpdateAsync(sprint, cancellationToken);
            }, cancellationToken);

            var dto = await sprintsReadModel.GetByIdAsync(tenantId, sprint.Id, request.ProjectId, cancellationToken);

            return dto is null
                ? Result<SprintDto>.Failure("Sprint completed but could not be loaded.", HttpStatusCode.InternalServerError)
                : Result<SprintDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}
