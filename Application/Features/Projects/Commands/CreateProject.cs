using Application.Common;
using Application.Features.Projects.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
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
        IUnitOfWork unitOfWork
    ) : IRequestHandler<Command, Result<ProjectDto>>
    {
        public async Task<Result<ProjectDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<ProjectDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var createdByUserId = currentUserAccessor.GetUserId();
            if (string.IsNullOrWhiteSpace(createdByUserId))
                return Result<ProjectDto>.Failure("User not found.", HttpStatusCode.NotFound);

            if (request.StartDate.HasValue && request.EndDate.HasValue && request.EndDate < request.StartDate)
                return Result<ProjectDto>.Failure("EndDate cannot be before StartDate.", HttpStatusCode.BadRequest);

            var isNameUnique = await projectsRepository.IsNameUniqueAsync(tenantId, request.Name, cancellationToken);
            if (!isNameUnique)
                return Result<ProjectDto>.Failure("A project with this name already exists.", HttpStatusCode.BadRequest);
            
            var isKeyUnique = await projectsRepository.IsKeyUniqueAsync(tenantId, request.Key, cancellationToken);
            if (!isKeyUnique)
                return Result<ProjectDto>.Failure("Project Key must be unique.", HttpStatusCode.BadRequest);

            if (!string.IsNullOrWhiteSpace(request.TeamId))
            {
                var team = await teamsRepository.GetByIdAsync(tenantId, request.TeamId, cancellationToken);
                if (team is null)
                    return Result<ProjectDto>.Failure("Team not found in this tenant.", HttpStatusCode.NotFound);

                if (!team.IsActive)
                    return Result<ProjectDto>.Failure("Cannot assign project to an inactive team.", HttpStatusCode.BadRequest);
            }

            Project? project = null;
            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                project = new Project(
                    name: request.Name,
                    key: request.Key,
                    createdByUserId: createdByUserId,
                    teamId: request.TeamId,
                    tenantId: tenantId
                );

                project.UpdateDetails(
                    name: request.Name,
                    description: request.Description,
                    startDate: request.StartDate,
                    endDate: request.EndDate
                );
                await projectsRepository.AddAsync(project, cancellationToken);

                historyRecorder.RecordCreationChanges<Project>(project.Id, project.Id, project.Name, project.Key);
            }, cancellationToken);

            var dto = await projectsReadModel.GetByIdAsync(tenantId, project.Id, cancellationToken);

            return dto is null
                ? Result<ProjectDto>.Failure("Project created but could not be loaded.", HttpStatusCode.InternalServerError)
                : Result<ProjectDto>.Success(dto, HttpStatusCode.Created);
        }
    }
}