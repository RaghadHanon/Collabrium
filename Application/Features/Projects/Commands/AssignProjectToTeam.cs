using Application.Common;
using Application.Features.Projects.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Domain.Enums;
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
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<Command, Result<ProjectDto>>
    {
        public async Task<Result<ProjectDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<ProjectDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var project = await projectsRepository.GetByIdAsync(tenantId, request.ProjectId, cancellationToken);
            if (project is null)
                return Result<ProjectDto>.Failure("Project not found.", HttpStatusCode.NotFound);

            var team = await teamsRepository.GetByIdAsync(tenantId, request.TeamId, cancellationToken);
            if (team is null)
                return Result<ProjectDto>.Failure("Team not found in this tenant.", HttpStatusCode.NotFound);

            if (!team.IsActive)
                return Result<ProjectDto>.Failure("Cannot assign project to an inactive team.", HttpStatusCode.BadRequest);

            var oldProjectState = project.Clone();
            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                project.AssignToTeam(request.TeamId);
                await projectsRepository.UpdateAsync(project, ct); 

                historyRecorder.RecordUpdatingChanges<Project>(project.Id, project.Id, project.Name, project.Key, oldProjectState, project);
            }, cancellationToken);

            var dto = await projectsReadModel.GetByIdAsync(tenantId, project.Id, cancellationToken);
            return dto is null
                ? Result<ProjectDto>.Failure("Project updated but could not be loaded.", HttpStatusCode.InternalServerError)
                : Result<ProjectDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}