using Application.Common;
using Application.Events;
using Application.Features.Projects.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.Projects.Commands;

public class CreateProject
{
    public class Command : IRequest<Result<ProjectDto>>
    {
        public required string Name { get; set; }
        public required string Key { get; set; }
        public string? Description { get; set; }
        public string? TeamId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class Handler(
        IProjectsRepository projectsRepository,
        IProjectHistoryRecorder historyRecorder,
        IProjectsReadModel projectsReadModel,
        ITeamsRepository teamsRepository,
        ITenantContext tenantContext,
        ICurrentUserAccessor currentUserAccessor,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ITeamsReadModel teamsReadModel
    ) : IRequestHandler<Command, Result<ProjectDto>>
    {
        public async Task<Result<ProjectDto>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id ?? tenantContext.TenantId;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<ProjectDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var currentUser = await currentUserAccessor.GetCurrentUserAsync();
            if (currentUser == null)
                return Result<ProjectDto>.Failure("Current user is not found.", HttpStatusCode.NotFound);

            if (request.StartDate.HasValue &&
                request.EndDate.HasValue &&
                request.EndDate < request.StartDate)
            {
                return Result<ProjectDto>.Failure(
                    "EndDate cannot be before StartDate.",
                    HttpStatusCode.BadRequest);
            }

            var isNameUnique = await projectsRepository.IsNameUniqueAsync(
                tenantId,
                request.Name,
                cancellationToken);

            if (!isNameUnique)
                return Result<ProjectDto>.Failure(
                    "A project with this name already exists.",
                    HttpStatusCode.BadRequest);

            var isKeyUnique = await projectsRepository.IsKeyUniqueAsync(
                tenantId,
                request.Key,
                cancellationToken);

            if (!isKeyUnique)
                return Result<ProjectDto>.Failure(
                    "Project Key must be unique.",
                    HttpStatusCode.BadRequest);

            Team? team = null;

            var hasTeam = !string.IsNullOrWhiteSpace(request.TeamId);

            if (hasTeam)
            {
                request.TeamId = request.TeamId!.Trim();

                team = await teamsRepository.GetByIdAsync(
                    tenantId,
                    request.TeamId,
                    cancellationToken);

                if (team is null)
                    return Result<ProjectDto>.Failure(
                        "Team not found in this tenant.",
                        HttpStatusCode.NotFound);

                if (!team.IsActive)
                    return Result<ProjectDto>.Failure(
                        "Cannot assign project to an inactive team.",
                        HttpStatusCode.BadRequest);
            }

            Project? project = null;

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                project = new Project(
                    name: request.Name,
                    key: request.Key,
                    createdByUserId: currentUser.Id,
                    teamId: hasTeam ? request.TeamId : null,
                    tenantId: tenantId
                );

                project.UpdateDetails(
                    name: request.Name,
                    description: request.Description,
                    startDate: request.StartDate,
                    endDate: request.EndDate
                );

                await projectsRepository.AddAsync(project, ct);

                historyRecorder.RecordCreationChanges<Project>(
                    project.Id,
                    project.Id,
                    project.Name,
                    project.Key);

            }, cancellationToken);

            if (project is null)
                return Result<ProjectDto>.Failure(
                    "Project could not be created.",
                    HttpStatusCode.InternalServerError);

            var dto = await projectsReadModel.GetByIdAsync(
                tenantId,
                project.Id,
                cancellationToken);

            if (dto is null)
                return Result<ProjectDto>.Failure(
                    "Project created but could not be loaded.",
                    HttpStatusCode.InternalServerError);

            if (hasTeam && team is not null)
            {
                var teamMembers = await teamsReadModel.GetTeamMembersSnapshotAsync(
                    tenantId,
                    request.TeamId!,
                    cancellationToken);

                await mediator.Publish(new ProjectAssignedEvent(
                        tenantId,
                        dto.Id,
                        dto.Name,
                        dto.Key,
                        teamMembers,
                        currentUser.Id,
                        currentUser.FullName,
                        team.Name
                    ), cancellationToken);
            }

            return Result<ProjectDto>.Success(dto, HttpStatusCode.Created);
        }
    }
}