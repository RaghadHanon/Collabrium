using Application.Common;
using Application.Features.Sprints.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.Sprints.Commands;

public class CreateSprint
{
    public class Command : IRequest<Result<SprintDto>>
    {
        public required string ProjectId { get; set; }
        public required string Name { get; set; }
        public required string Goal { get; set; }
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
    }

    public class Handler(
        ISprintsRepository sprintsRepository,
        ISprintsReadModel sprintsReadModel,
        IProjectsRepository projectsRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork) : IRequestHandler<Command, Result<SprintDto>>
    {
        public async Task<Result<SprintDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<SprintDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var project = await projectsRepository.GetByIdAsync(tenantId, request.ProjectId, cancellationToken);
            if (project is null)
                return Result<SprintDto>.Failure("Project not found in this tenant.", HttpStatusCode.NotFound);

            Sprint sprint = null!;

            await unitOfWork.ExecuteInTransactionAsync(async cancellationToken =>
            {
                sprint = new Sprint(
                    tenantId: tenantId,
                    projectId: request.ProjectId,
                    name: request.Name,
                    goal: request.Goal,
                    start: request.StartDate,
                    end: request.EndDate);

                await sprintsRepository.AddAsync(sprint, cancellationToken);
            }, cancellationToken);

            var dto = await sprintsReadModel.GetByIdAsync(tenantId, sprint.Id, cancellationToken);

            return dto is null
                ? Result<SprintDto>.Failure("Sprint created but could not be loaded.", HttpStatusCode.InternalServerError)
                : Result<SprintDto>.Success(dto, HttpStatusCode.Created);
        }
    }
}