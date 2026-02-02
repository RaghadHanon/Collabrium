using Application.Common;
using Application.Features.Projects.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.Projects.Commands;

public class UpdateProject
{
    public class Command : IRequest<Result<ProjectDto>>
    {
        public required string ProjectId { get; set; }

        public string? Name { get; set; }
        public string? Description { get; set; }

        public ProjectStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class Handler(
        IProjectsRepository projectsRepository,
        IProjectsReadModel projectsReadModel,
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

            if (!string.IsNullOrWhiteSpace(request.Name) &&
                !string.Equals(project.Name, request.Name, StringComparison.OrdinalIgnoreCase))
            {
                var isUnique = await projectsRepository.IsNameUniqueAsync(tenantId, request.Name, cancellationToken);
                if (!isUnique)
                    return Result<ProjectDto>.Failure("A project with this name already exists.", HttpStatusCode.BadRequest);
            }

            var newStart = request.StartDate ?? project.StartDate;
            var newEnd = request.EndDate ?? project.EndDate;

            project.UpdateDetails(
                name: request.Name ?? project.Name,
                description: request.Description ?? project.Description,
                status: request.Status ?? project.Status,
                startDate: newStart,
                endDate: newEnd
            );

            await projectsRepository.UpdateAsync(project, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = await projectsReadModel.GetByIdAsync(tenantId, project.Id, cancellationToken);

            return dto is null
                ? Result<ProjectDto>.Failure("Project updated but could not be loaded.", HttpStatusCode.InternalServerError)
                : Result<ProjectDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}