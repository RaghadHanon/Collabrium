using Application.Common;
using Application.Features.Sprints.DTOs;
using Application.Features.WorkItems.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.WorkItems.Queries;

public sealed class GetWorkItems
{
    public sealed class Query : IRequest<Result<PagedList<WorkItemListItemDto, DateTime?>>>
    {
        public required string ProjectId { get; set; }
        public WorkItemsSearchAndFilterParametersDto Parameters { get; set; } = new();
    }

    public sealed class Handler(
        IWorkItemsReadModel workItemsReadModel,
        IProjectsRepository projectsRepository,
        ITenantContext tenantContext) : IRequestHandler<Query, Result<PagedList<WorkItemListItemDto, DateTime?>>>
    {
        public async Task<Result<PagedList<WorkItemListItemDto, DateTime?>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<PagedList<WorkItemListItemDto, DateTime?>>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            if (!(await projectsRepository.IsProjectExists(tenantId, request.ProjectId, cancellationToken)))
                return Result< PagedList<WorkItemListItemDto, DateTime?>>.Failure("Project not found.", HttpStatusCode.NotFound);

            request.Parameters.TenantId = tenantId;
            request.Parameters.ProjectId = request.ProjectId;

            var list = await workItemsReadModel.GetWorkItemsAsync(request.Parameters, cancellationToken);
            return Result<PagedList<WorkItemListItemDto, DateTime?>>.Success(list, HttpStatusCode.OK);
        }
    }
}