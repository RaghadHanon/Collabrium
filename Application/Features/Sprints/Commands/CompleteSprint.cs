using Application.Common;
using Application.Events;
using Application.Features.Sprints.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Models;
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
        IWorkItemsRepository workItemsRepository,
        IProjectHistoryRecorder historyRecorder,
        ITeamsReadModel teamsReadModel,
        ISprintsReadModel sprintsReadModel,
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

            var sprint = await sprintsRepository.GetByIdAsync(tenantId, request.SprintId, request.ProjectId, cancellationToken);
            if (sprint is null)
                return Result<SprintDto>.Failure("Sprint not found.", HttpStatusCode.NotFound);

            if (!await sprintsRepository.CanSprintBeCompletedAsync(tenantId, request.SprintId, cancellationToken))
                return Result<SprintDto>.Failure("Sprint cannot be completed. Please ensure all work items are closed.",
                    HttpStatusCode.BadRequest);

            var oldSprintState = sprint.Clone();
            await unitOfWork.ExecuteInTransactionAsync((Func<CancellationToken, Task>) (async cancellationToken =>
            {
                await workItemsRepository.MoveWorkItemsFromSprintToBacklogAsync(tenantId, sprint.Id, sprint.ProjectId, cancellationToken);
                sprint.Complete();
                await sprintsRepository.UpdateAsync(sprint, cancellationToken);

                await historyRecorder.RecordUpdatingChangesAsync(
                     projectId: sprint.ProjectId,
                     relatedEntityId: sprint.Id,
                     relatedEntityTitle: sprint.Name ?? sprint.Code,
                     relatedEntityKey: sprint.Code,
                     oldObject: oldSprintState,
                     newObject: sprint,
                     cancellationToken: cancellationToken);
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
                ? Result<SprintDto>.Failure("Sprint completed but could not be loaded.", HttpStatusCode.InternalServerError)
                : Result<SprintDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}
