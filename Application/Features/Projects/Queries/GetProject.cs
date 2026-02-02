using Application.Common;
using Application.Features.Projects.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using MediatR;
using System.Net;

namespace Application.Features.Projects.Queries;

public class GetProject
{
    public class Query : IRequest<Result<ProjectDto>>
    {
        public required string ProjectId { get; set; }
    }

    public class Handler(
        IProjectsReadModel projectsReadModel,
        ITenantContext tenantContext
    ) : IRequestHandler<Query, Result<ProjectDto>>
    {
        public async Task<Result<ProjectDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<ProjectDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var dto = await projectsReadModel.GetByIdAsync(tenantId, request.ProjectId, cancellationToken);
            return dto is null
                ? Result<ProjectDto>.Failure("Project not found.", HttpStatusCode.NotFound)
                : Result<ProjectDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}