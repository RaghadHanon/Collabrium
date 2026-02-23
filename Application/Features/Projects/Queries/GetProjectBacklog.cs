using Application.Common;
using Application.Features.WorkItems.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using MediatR;
using System.Net;

namespace Application.Features.Projects.Queries;

public class GetProjectBacklog
{
    public class Query : IRequest<Result<IReadOnlyList<WorkItemListItemDto>>>
    {
        public required string ProjectId { get; set; }
    }

    public class Handler(
        IProjectsReadModel projectsReadModel,
        ITenantContext tenantContext
    ) : IRequestHandler<Query, Result<IReadOnlyList<WorkItemListItemDto>>>
    {
        public async Task<Result<IReadOnlyList<WorkItemListItemDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<IReadOnlyList<WorkItemListItemDto>>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var items = await projectsReadModel.GetBacklogAsync(tenantId, request.ProjectId, cancellationToken);
            return Result<IReadOnlyList<WorkItemListItemDto>>.Success(items, HttpStatusCode.OK);
        }
    }
}
