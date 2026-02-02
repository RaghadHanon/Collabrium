using Application.Common;
using Application.Features.Projects.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Projects.Commands;

public class UnassignProjectFromTeam
{
    public class Command : IRequest<Result<ProjectDto>>
    {
        public required string ProjectId { get; set; }
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

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                project.UnassignFromTeam();
                await projectsRepository.UpdateAsync(project, ct);
                await unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);

            var dto = await projectsReadModel.GetByIdAsync(tenantId, project.Id, cancellationToken);
            return dto is null
                ? Result<ProjectDto>.Failure("Project updated but could not be loaded.", HttpStatusCode.InternalServerError)
                : Result<ProjectDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}
