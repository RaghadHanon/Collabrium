using Application.Common;
using Application.Events;
using Application.Features.Sprints.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
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
        IUnitOfWork unitOfWork)
        : IRequestHandler<Command, Result<SprintDto>>
    {
        public async Task<Result<SprintDto>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var tenantId =
                tenantContext.CurrentTenant?.Id;

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return Result<SprintDto>.Failure(
                    "Tenant not found.",
                    HttpStatusCode.NotFound);
            }

            var currentUser =
                await currentUserAccessor.GetCurrentUserAsync();

            if (currentUser is null)
            {
                return Result<SprintDto>.Failure(
                    "Current user is not found.",
                    HttpStatusCode.NotFound);
            }

            var project =
                await projectsRepository.GetByIdAsync(
                    tenantId,
                    request.ProjectId,
                    cancellationToken);

            if (project is null)
            {
                return Result<SprintDto>.Failure(
                    "Project not found.",
                    HttpStatusCode.NotFound);
            }

            if (project.Status is not
                (ProjectStatus.InProgress or ProjectStatus.OnHold))
            {
                return Result<SprintDto>.Failure(
                    "Cannot start a sprint for a project that is not in progress or on hold.",
                    HttpStatusCode.BadRequest);
            }

            var sprint =
                await sprintsRepository.GetByIdAsync(
                    tenantId,
                    request.SprintId,
                    request.ProjectId,
                    cancellationToken);

            if (sprint is null)
            {
                return Result<SprintDto>.Failure(
                    "Sprint not found.",
                    HttpStatusCode.NotFound);
            }

            if (sprint.Status != SprintStatus.Planned)
            {
                return Result<SprintDto>.Failure(
                    "Only a planned sprint can be started.",
                    HttpStatusCode.BadRequest);
            }

            var hasWorkItems =
                await sprintsRepository.SprintHasWorkItemsAsync(
                    tenantId,
                    request.SprintId,
                    cancellationToken);

            if (!hasWorkItems)
            {
                return Result<SprintDto>.Failure(
                    "Cannot start a sprint without work items.",
                    HttpStatusCode.BadRequest);
            }

            var now = DateTime.UtcNow;

            if (request.StartDate > now)
            {
                return Result<SprintDto>.Failure(
                    "The sprint cannot be started before its planned start date.",
                    HttpStatusCode.BadRequest);
            }

            if (request.EndDate <= request.StartDate)
            {
                return Result<SprintDto>.Failure(
                    "Sprint end date must be greater than the start date.",
                    HttpStatusCode.BadRequest);
            }

            if (request.EndDate <= now)
            {
                return Result<SprintDto>.Failure(
                    "The sprint cannot be started because its planned end date has already passed.",
                    HttpStatusCode.BadRequest);
            }

            if (project.StartDate.HasValue &&
                request.StartDate < project.StartDate.Value)
            {
                return Result<SprintDto>.Failure(
                    "Sprint start date cannot be before the project start date.",
                    HttpStatusCode.BadRequest);
            }

            if (project.EndDate.HasValue &&
                request.EndDate > project.EndDate.Value)
            {
                return Result<SprintDto>.Failure(
                    "Sprint end date cannot be after the project end date.",
                    HttpStatusCode.BadRequest);
            }

            var updatedName =
                PatchHelpers.ApplyStringPatch(
                    sprint.Name,
                    request.Name);

            var updatedGoal =
                PatchHelpers.ApplyStringPatch(
                    sprint.Goal,
                    request.Goal);

            var oldSprintState =
                sprint.Clone();

            await unitOfWork.ExecuteInTransactionAsync(
                async transactionCancellationToken =>
                {
                    sprint.Start(
                        start: request.StartDate,
                        end: request.EndDate,
                        name: updatedName,
                        goal: updatedGoal);

                    await sprintsRepository.UpdateAsync(
                        sprint,
                        transactionCancellationToken);

                    await historyRecorder.RecordUpdatingChangesAsync(
                        projectId: sprint.ProjectId,
                        relatedEntityId: sprint.Id,
                        relatedEntityTitle: sprint.Name ?? sprint.Code,
                        relatedEntityKey: sprint.Code,
                        oldObject: oldSprintState,
                        newObject: sprint,
                        cancellationToken: transactionCancellationToken);
                },
                cancellationToken);

            var dto =
                await sprintsReadModel.GetByIdAsync(
                    tenantId,
                    sprint.Id,
                    request.ProjectId,
                    cancellationToken);

            if (dto is null)
            {
                return Result<SprintDto>.Failure(
                    "Sprint started but could not be loaded.",
                    HttpStatusCode.InternalServerError);
            }

            if (!string.IsNullOrWhiteSpace(project.TeamId))
            {
                var teamMembers =
                    await teamsReadModel.GetTeamMembersSnapshotAsync(
                        tenantId,
                        project.TeamId,
                        cancellationToken);

                await mediator.Publish(
                    new SprintStatusChangedEvent(
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
                        currentUser.FullName),
                    cancellationToken);
            }

            return Result<SprintDto>.Success(
                dto,
                HttpStatusCode.OK);
        }
    }
}