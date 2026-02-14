using Application.Common;
using Application.Features.Sprints.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.WorkItems.Queries;

public class GetWorkItemStatusesByType
{
    public class Query : IRequest<Result<List<WorkItemStatus>>>
    {
        public WorkItemType Type { get; set; }
    }

    public class Handler(
        ITenantContext tenantContext,
        IWorkItemsReadModel workItemsReadModel
      ) : IRequestHandler<Query, Result<List<WorkItemStatus>>>
    {
        public async Task<Result<List<WorkItemStatus>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<List<WorkItemStatus>>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var statuses = workItemsReadModel.GetAllowedStatusesByType(request.Type);

            return Result<List<WorkItemStatus>>.Success(statuses, HttpStatusCode.OK);
        }
    }
}