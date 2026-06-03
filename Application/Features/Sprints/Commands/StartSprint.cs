using Application.Common;
using Application.Events;
using Application.Features.Sprints.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.Sprints.Commands;

public class StartSprint
{
    public class Command : IRequest<Result<SprintDto>>
    {
        public required string SprintId { get; set; }
        public required string ProjectId { get; set; }
        public string? Name { get; set; }
        public string? Goal { get; set; }
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
    }

    public class Handler(
        ISprintsRepository sprintsRepository,
        IProjectHistoryRecorder historyRecorder,
        ISprintsReadModel sprintsReadModel,
        ITeamsReadModel teamsReadModel,
        ITenantContext tenantContext,
        IProjectsRepository projectsRepository,
        ICurrentUserAccessor currentUserAccessor,
        IMediator mediator,
        IUnitOfWork unitOfWork) : IRequestHandler<Command, Result<SprintDto>>
    {
        public async Task<Result<SprintDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<SprintDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var currentUser = await currentUserAccessor.GetCurrentUserAsync();
            if (currentUser == null)
                return Result<SprintDto>.Failure("Current user is not found.", HttpStatusCode.NotFound);

            var project = await projectsRepository.GetByIdAsync(tenantId, request.ProjectId, cancellationToken);
            if (project is null)
                return Result<SprintDto>.Failure("Project not found.", HttpStatusCode.NotFound);

            if (project.Status != ProjectStatus.InProgress && project.Status != ProjectStatus.OnHold)
                return Result<SprintDto>.Failure("Cannot start a sprint for a project that is not in progress or on hold.", HttpStatusCode.BadRequest);

            var sprint = await sprintsRepository.GetByIdAsync(tenantId, request.SprintId, request.ProjectId, cancellationToken);
            if (sprint is null)
                return Result<SprintDto>.Failure("Sprint not found.", HttpStatusCode.NotFound);

            if (!await sprintsRepository.SprintHasWorkItemsAsync(tenantId, request.SprintId, cancellationToken))
                return Result<SprintDto>.Failure("Cannot start a sprint for a project without work items.", HttpStatusCode.BadRequest);

            var oldSprintState = sprint.Clone();
            await unitOfWork.ExecuteInTransactionAsync((Func<CancellationToken, Task>) (async cancellationToken =>
            {
                sprint.Start(
                    request.StartDate,
                    request.EndDate,
                    PatchHelpers.ApplyStringPatch(sprint.Name, request.Name),
                    PatchHelpers.ApplyStringPatch(sprint.Goal, request.Goal)
                    );
                await sprintsRepository.UpdateAsync(sprint, cancellationToken);

                historyRecorder.RecordUpdatingChanges<Sprint>(sprint.ProjectId, sprint.Id, sprint.Name ?? sprint.Code, sprint.Code, oldSprintState, sprint);
            }), cancellationToken);

            var dto = await sprintsReadModel.GetByIdAsync(tenantId, sprint.Id, request.ProjectId, cancellationToken);
            var teamMembers = await teamsReadModel.GetTeamMembersSnapshotAsync(tenantId, project.TeamId, cancellationToken);
            if (project.TeamId is not null)
                await mediator.Publish(new SprintStatusChangedEvent(
                    tenantId,
                    project.Id,
                    project.Name,
                    project.Key,
                    sprint.Id,
                    sprint.Code,
                    sprint.Name,
                    sprint.Status,
                    teamMembers,
                    currentUser.Id,
                    currentUser.FullName
                ), cancellationToken);

            return dto is null
                ? Result<SprintDto>.Failure("Sprint started but could not be loaded.", HttpStatusCode.InternalServerError)
                : Result<SprintDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}