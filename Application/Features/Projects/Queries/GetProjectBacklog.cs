using Application.Common;
using Application.Features.WorkItems.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using AutoMapper;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.Projects.Queries;

public class GetProjectBacklog
{
    public class Query : IRequest<Result<PagedList<WorkItemListItemDto, DateTime?>>>
    {
        public required string ProjectId { get; set; }
        public WorkItemType? Type { get; init; }
        public WorkItemStatus? Status { get; init; }
        public WorkItemPriority? Priority { get; init; }
        public string? AssignedToUserId { get; init; }
        public string? CreatedByUserId { get; init; }
        public string? ParentId { get; init; }
        public bool? IsAssigned { get; set; }
        public string? TitleContains { get; init; }
        public string? Code { get; set; }
        public DateTime? CreatedAfter { get; init; }
        public DateTime? CreatedBefore { get; init; }
        public int PageSize { get; init; }
        public DateTime? Cursor { get; init; }
    }

    public class Handler(
        IWorkItemsReadModel workItemsReadModel,
        ITenantContext tenantContext,
        IMapper mapper
    ) : IRequestHandler<Query, Result<PagedList<WorkItemListItemDto, DateTime?>>>
    {
        public async Task<Result<PagedList<WorkItemListItemDto, DateTime?>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<PagedList<WorkItemListItemDto, DateTime?>>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var parameters = mapper.Map<WorkItemsSearchAndFilterParametersDto>(request);
            parameters.TenantId = tenantId;

            var items = await workItemsReadModel.GetBacklogAsync(tenantId, request.ProjectId, parameters, cancellationToken);
            return Result<PagedList<WorkItemListItemDto, DateTime?>>.Success(items, HttpStatusCode.OK);
        }
    }
}
