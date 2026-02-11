using Application.Common;
using Application.Features.WorkItems.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.Projects.Queries;

public class GetWorkItemsByType
{
    public class Query : IRequest<Result<List<WorkItemListItemDto>>>
    {
        public required string ProjectId { get; set; }
        public string? SprintId { get; set; }
        public WorkItemType Type { get; set; }
    }

    public class Handler(
        IWorkItemsReadModel workItemsReadModel,
        ITenantContext tenantContext) : IRequestHandler<Query, Result<List<WorkItemListItemDto>>>
    {
        public async Task<Result<List<WorkItemListItemDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<List<WorkItemListItemDto>>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var items = await workItemsReadModel.GetWorkItemsByTypeAsync(
                tenantId: tenantId,
                projectId: request.ProjectId,
                type: request.Type,
                sprintId: request.SprintId,
                cancellationToken: cancellationToken);

            return Result<List<WorkItemListItemDto>>.Success(items, HttpStatusCode.OK);
        }
    }
}