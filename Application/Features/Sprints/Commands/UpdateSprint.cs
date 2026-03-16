using Application.Common;
using Application.Features.Sprints.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.Sprints.Commands;

public class UpdateSprint
{
    public class Command : IRequest<Result<SprintDto>>
    {
        public required string SprintId { get; set; }
        public required string ProjectId { get; set; }
        public string? Name { get; set; }
        public string? Goal { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class Handler(
        ISprintsRepository sprintsRepository,
        IProjectHistoryRecorder historyRecorder,
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

            var project = await projectsRepository.GetByIdAsync(tenantId, request.ProjectId, cancellationToken);
            if (project is null)
                return Result<SprintDto>.Failure("Project not found.", HttpStatusCode.NotFound);

            var sprint = await sprintsRepository.GetByIdAsync(
                tenantId, request.SprintId, request.ProjectId, cancellationToken);

            if (sprint is null)
                return Result<SprintDto>.Failure("Sprint not found.", HttpStatusCode.NotFound);

            if (project.StartDate.HasValue && request.StartDate < project.StartDate.Value)
                return Result<SprintDto>.Failure("Sprint start date cannot be before the project start date.", HttpStatusCode.BadRequest);

            if (project.EndDate.HasValue && request.EndDate > project.EndDate.Value)
                return Result<SprintDto>.Failure("Sprint end date cannot be after the project end date.", HttpStatusCode.BadRequest);

            var newStart = request.StartDate ?? sprint.StartDate;
            var newEnd = request.EndDate ?? sprint.EndDate;

            if (newEnd < newStart)
                return Result<SprintDto>.Failure("Sprint end date must be >= start date.", HttpStatusCode.BadRequest);

            var oldSprintState = sprint.Clone();
            await unitOfWork.ExecuteInTransactionAsync(async cancellationToken =>
            {
                sprint.Update(
                    start: newStart,
                    end: newEnd,
                    PatchHelpers.ApplyStringPatch(sprint.Name, request.Name),
                    PatchHelpers.ApplyStringPatch(sprint.Goal, request.Goal)
                    );

                await sprintsRepository.UpdateAsync(sprint, cancellationToken);

                historyRecorder.RecordUpdatingChanges<Sprint>(sprint.ProjectId, sprint.Id, sprint.Code, oldSprintState, sprint);
            }, cancellationToken);

            var dto = await sprintsReadModel.GetByIdAsync(
                tenantId, sprint.Id, request.ProjectId, cancellationToken);

            return dto is null
                ? Result<SprintDto>.Failure("Sprint updated but could not be loaded.", HttpStatusCode.InternalServerError)
                : Result<SprintDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}