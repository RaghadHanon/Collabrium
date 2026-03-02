using Application.Common;
using Application.Features.Sprints.DTOs;
using Application.Features.WorkItems.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.WorkItems.Queries;

public sealed class GetWorkItems
{
    public sealed class Query : IRequest<Result<PagedList<WorkItemListItemDto, DateTime?>>>
    {
        public required string ProjectId { get; set; }

        public string? TenantId { get; set; }

        public WorkItemType? Type { get; set; }
        public WorkItemStatus? Status { get; set; }
        public WorkItemPriority? Priority { get; set; }
        public string? Code { get; set; }

        public string? SprintId { get; set; }
        public string? AssignedToUserId { get; set; }

        public string? TitleContains { get; set; }

        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }

        public int PageSize { get; init; } 
        public DateTime? Cursor { get; init; }
    }

    public sealed class Handler(
        IWorkItemsReadModel workItemsReadModel,
        IProjectsRepository projectsRepository,
        IMapper mapper,
        ITenantContext tenantContext) : IRequestHandler<Query, Result<PagedList<WorkItemListItemDto, DateTime?>>>
    {
        public async Task<Result<PagedList<WorkItemListItemDto, DateTime?>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<PagedList<WorkItemListItemDto, DateTime?>>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            if (!(await projectsRepository.IsProjectExists(tenantId, request.ProjectId, cancellationToken)))
                return Result< PagedList<WorkItemListItemDto, DateTime?>>.Failure("Project not found.", HttpStatusCode.NotFound);

            var parameters = mapper.Map<WorkItemsSearchAndFilterParametersDto>(request);
            parameters.TenantId = tenantId;

            var list = await workItemsReadModel.GetWorkItemsAsync(parameters, cancellationToken);
            return Result<PagedList<WorkItemListItemDto, DateTime?>>.Success(list, HttpStatusCode.OK);
        }
    }
}