using Application.Common;
using Application.Features.WorkItems.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using MediatR;
using System.Net;

namespace Application.Features.Projects.Queries;

public class GetProjectBacklog
{
    public class Query : IRequest<Result<IReadOnlyList<WorkItemDto>>>
    {
        public required string ProjectId { get; set; }
    }

    public class Handler(
        IProjectsReadModel projectsReadModel,
        ITenantContext tenantContext
    ) : IRequestHandler<Query, Result<IReadOnlyList<WorkItemDto>>>
    {
        public async Task<Result<IReadOnlyList<WorkItemDto>>> Handle(Query request, 
            CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<IReadOnlyList<WorkItemDto>>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var items = await projectsReadModel.GetBacklogAsync(tenantId, request.ProjectId, cancellationToken);
            return Result<IReadOnlyList<WorkItemDto>>.Success(items, HttpStatusCode.OK);
        }
    }
}
