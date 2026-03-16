using Application.Common;
using Application.Features.Sprints.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Domain.Enums;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.Sprints.Commands;

public class CreateSprint
{
    public class Command : IRequest<Result<SprintDto>>
    {
        public required string ProjectId { get; set; }
    }

    public class Handler(
        ISprintsRepository sprintsRepository,
        IProjectHistoryRecorder historyRecorder,
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
                return Result<SprintDto>.Failure("Project not found.", HttpStatusCode.NotFound);

            if (project.Status == ProjectStatus.Completed || project.Status == ProjectStatus.Cancelled)
                return Result<SprintDto>.Failure($"Cannot create a sprint for an {project.Status} project.", HttpStatusCode.BadRequest);

            Sprint sprint = null!;
            await unitOfWork.ExecuteInTransactionAsync((Func<CancellationToken, Task>) (async cancellationToken =>
            {
                sprint = new Sprint(
                    tenantId: tenantId,
                    projectId: project.Id,
                    code: project.GenerateNextSprintName());

                sprintsRepository.Add(sprint, cancellationToken);
                await projectsRepository.UpdateAsync(project, cancellationToken);

                historyRecorder.RecordCreationChanges<Sprint>(sprint.ProjectId, sprint.Id, sprint.Name ?? sprint.Code, sprint.Code);
            }), cancellationToken);

            var dto = await sprintsReadModel.GetByIdAsync(tenantId, sprint.Id, request.ProjectId, cancellationToken);

            return dto is null
                ? Result<SprintDto>.Failure("Sprint created but could not be loaded.", HttpStatusCode.InternalServerError)
                : Result<SprintDto>.Success(dto, HttpStatusCode.Created);
        }
    }
}