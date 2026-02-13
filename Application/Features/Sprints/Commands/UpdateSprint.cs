using Application.Common;
using Application.Features.Sprints.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
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

            if(!(await projectsRepository.IsProjectExists(tenantId, request.ProjectId, cancellationToken)))
                return Result<SprintDto>.Failure("Project not found.", HttpStatusCode.NotFound);

            var sprint = await sprintsRepository.GetByIdAsync(
                tenantId, request.SprintId, request.ProjectId, cancellationToken);

            if (sprint is null)
                return Result<SprintDto>.Failure("Sprint not found.", HttpStatusCode.NotFound);

            var newStart = request.StartDate ?? sprint.StartDate;
            var newEnd = request.EndDate ?? sprint.EndDate;

            if (newEnd < newStart)
                return Result<SprintDto>.Failure("Sprint end date must be >= start date.", HttpStatusCode.BadRequest);

            await unitOfWork.ExecuteInTransactionAsync(async cancellationToken =>
            {
                sprint.UpdateDetails(
                    name: PatchHelpers.ApplyStringPatch(sprint.Name, request.Name) ?? sprint.Name,
                    goal: PatchHelpers.ApplyStringPatch(sprint.Goal, request.Goal) ?? sprint.Goal,
                    start: newStart,
                    end: newEnd);

                await sprintsRepository.UpdateAsync(sprint, cancellationToken);
            }, cancellationToken);

            var dto = await sprintsReadModel.GetByIdAsync(
                tenantId, sprint.Id, request.ProjectId, cancellationToken);

            return dto is null
                ? Result<SprintDto>.Failure("Sprint updated but could not be loaded.", HttpStatusCode.InternalServerError)
                : Result<SprintDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}