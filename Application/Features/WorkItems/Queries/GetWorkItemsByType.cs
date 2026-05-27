using Application.Common;
using Application.Features.WorkItems.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.WorkItems.Queries;

public class GetWorkItemsByType
{
    public class Query : IRequest<Result<PagedList<WorkItemListItemDto, DateTime?>>>
    {
        public required string ProjectId { get; set; }
        public string? SprintId { get; set; }
        public WorkItemType Type { get; set; }
        public WorkItemStatus? Status { get; init; }
        public WorkItemPriority? Priority { get; init; }
        public string? AssignedToUserId { get; init; }
        public string? CreatedByUserId { get; init; }
        public string? ParentId { get; init; }
        public string? TitleContains { get; init; }
        public string? Code { get; set; }
        public DateTime? CreatedAfter { get; init; }
        public DateTime? CreatedBefore { get; init; }
        public bool? IsAssigned { get; set; }
        public int PageSize { get; init; } = 10;
        public DateTime? Cursor { get; init; }
    }

    public class Handler(
        IWorkItemsReadModel workItemsReadModel,
        ITenantContext tenantContext,
        IProjectsRepository projectsRepository,
        ISprintsRepository sprintsRepository,
        IMapper mapper
        ) : IRequestHandler<Query, Result<PagedList<WorkItemListItemDto, DateTime?>>>
    {
        public async Task<Result<PagedList<WorkItemListItemDto, DateTime?>>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;

            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<PagedList<WorkItemListItemDto, DateTime?>>.Failure(
                    "Tenant not found.",
                    HttpStatusCode.NotFound);

            if (!await projectsRepository.IsProjectExists(tenantId, request.ProjectId, cancellationToken))
                return Result<PagedList<WorkItemListItemDto, DateTime?>>.Failure(
                    "Project not found.",
                    HttpStatusCode.NotFound);

            if (!string.IsNullOrWhiteSpace(request.SprintId) &&
                !await sprintsRepository.IsExistsAsync(tenantId, request.SprintId, request.ProjectId))
            {
                return Result<PagedList<WorkItemListItemDto, DateTime?>>.Failure(
                    "Sprint not found in this project.",
                    HttpStatusCode.NotFound);
            }

            var parameters = mapper.Map<WorkItemsSearchAndFilterParametersDto>(request);

            parameters.TenantId = tenantId;
            parameters.ProjectId = request.ProjectId;
            parameters.Type = request.Type;

            var items = await workItemsReadModel.GetWorkItemsByTypeAsync(
                tenantId: tenantId,
                projectId: request.ProjectId,
                type: request.Type,
                parameters: parameters,
                cancellationToken: cancellationToken);

            return Result<PagedList<WorkItemListItemDto, DateTime?>>.Success(
                items,
                HttpStatusCode.OK);
        }
    }
}