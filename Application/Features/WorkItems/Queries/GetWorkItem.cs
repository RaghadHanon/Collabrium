using Application.Common;
using Application.Features.Sprints.DTOs;
using Application.Features.WorkItems.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.WorkItems.Queries;

public class GetWorkItem
{
    public class Query : IRequest<Result<WorkItemDto>>
    {
        public required string ProjectId { get; set; }
        public required string WorkItemId { get; set; }
    }

    public class Handler(
        IWorkItemsReadModel workItemsReadModel,
        IProjectsRepository projectsRepository,
        ITenantContext tenantContext) : IRequestHandler<Query, Result<WorkItemDto>>
    {
        public async Task<Result<WorkItemDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<WorkItemDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            if (!(await projectsRepository.IsProjectExists(tenantId, request.ProjectId, cancellationToken)))
                return Result<WorkItemDto>.Failure("Project not found.", HttpStatusCode.NotFound);

            var dto = await workItemsReadModel.GetByIdAsync(tenantId, request.ProjectId, request.WorkItemId, cancellationToken);

            return dto is null
                ? Result<WorkItemDto>.Failure("Work item not found in this project.", HttpStatusCode.NotFound)
                : Result<WorkItemDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}