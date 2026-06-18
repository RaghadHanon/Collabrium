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

public class AssignProjectToTeam
{
    public class Command : IRequest<Result<ProjectDto>>
    {
        public required string ProjectId { get; set; }
        public required string TeamId { get; set; }
    }

    public class Handler(
        IProjectsRepository projectsRepository,
        IProjectsReadModel projectsReadModel,
        IProjectHistoryRecorder historyRecorder,
        ITeamsRepository teamsRepository,
        ITeamsReadModel teamsReadModel,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        ICurrentUserAccessor currentUserAccessor,
        IMediator mediator
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

            var project = await projectsRepository.GetByIdAsync(
                tenantId,
                request.ProjectId,
                cancellationToken);

            if (project is null)
                return Result<ProjectDto>.Failure("Project not found.", HttpStatusCode.NotFound);

            var team = await teamsRepository.GetByIdAsync(
                tenantId,
                request.TeamId,
                cancellationToken);

            if (team is null)
                return Result<ProjectDto>.Failure("Team not found in this tenant.", HttpStatusCode.NotFound);

            if (!team.IsActive)
                return Result<ProjectDto>.Failure(
                    "Cannot assign project to an inactive team.",
                    HttpStatusCode.BadRequest);

            var oldProjectState = project.Clone();

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                project.AssignToTeam(request.TeamId);

                await projectsRepository.UpdateAsync(project, ct);

                await historyRecorder.RecordUpdatingChangesAsync(
                    projectId: project.Id,
                    relatedEntityId: project.Id,
                    relatedEntityTitle: project.Name,
                    relatedEntityKey: project.Key,
                    oldObject: oldProjectState,
                    newObject: project,
                    cancellationToken: ct);

            }, cancellationToken);

            var dto = await projectsReadModel.GetByIdAsync(
                tenantId,
                project.Id,
                cancellationToken);

            if (dto is null)
                return Result<ProjectDto>.Failure(
                    "Project updated but could not be loaded.",
                    HttpStatusCode.InternalServerError);

            var teamMembers = await teamsReadModel.GetTeamMembersSnapshotAsync(
                tenantId,
                request.TeamId,
                cancellationToken);

            await mediator.Publish(new ProjectAssignedEvent(
                    tenantId,
                    request.ProjectId,
                    dto.Name,
                    dto.Key,
                    teamMembers,
                    currentUser.Id,
                    currentUser.FullName,
                    team.Name
                ), cancellationToken);

            return Result<ProjectDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}