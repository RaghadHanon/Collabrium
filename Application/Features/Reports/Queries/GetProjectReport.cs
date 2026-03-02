using Application.Common;
using Application.Features.Reports.DTOs;
using Application.Features.WorkItems.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using MediatR;
using System.Net;

namespace Application.Features.Reports.Queries;

public sealed class GetProjectReport
{
    public sealed class Query : IRequest<Result<ProjectReportDto>>
    {
        public required string ProjectId { get; set; }
    }

    public sealed class Handler(
        ITenantContext tenantContext,
        IProjectsReadModel projectsReadModel,
        IWorkItemsReadModel workItemsReadModel) : IRequestHandler<Query, Result<ProjectReportDto>>
    {
        private const int MaxPageSize = 50;

        public async Task<Result<ProjectReportDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<ProjectReportDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var project = await projectsReadModel.GetByIdAsync(tenantId, request.ProjectId, cancellationToken);
            if (project is null)
                return Result<ProjectReportDto>.Failure("Project not found.", HttpStatusCode.NotFound);

            var allWorkItems = new List<WorkItemListItemDto>();
            DateTime? cursor = null;

            do
            {
                var page = await workItemsReadModel.GetWorkItemsAsync(new WorkItemsSearchAndFilterParametersDto
                {
                    TenantId = tenantId,
                    ProjectId = request.ProjectId,
                    Cursor = cursor,
                    PageSize = MaxPageSize
                }, cancellationToken);

                allWorkItems.AddRange(page.Items);
                cursor = page.NextCursor;
            }
            while (cursor.HasValue);

            var report = new ProjectReportDto
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                GeneratedAtUtc = DateTime.UtcNow,
                TotalWorkItems = allWorkItems.Count,
                BlockedWorkItems = allWorkItems.Count(w => w.IsBlocked),
                UnassignedWorkItems = allWorkItems.Count(w => string.IsNullOrWhiteSpace(w.AssignedToUserId)),
                BacklogWorkItems = allWorkItems.Count(w => string.IsNullOrWhiteSpace(w.SprintId)),
                ByType = allWorkItems
                    .GroupBy(w => w.Type)
                    .OrderBy(g => g.Key)
                    .Select(g => new WorkItemsCountByTypeDto
                    {
                        Type = g.Key,
                        Count = g.Count()
                    })
                    .ToList(),
                ByStatus = allWorkItems
                    .GroupBy(w => w.Status)
                    .OrderBy(g => g.Key)
                    .Select(g => new WorkItemsCountByStatusDto
                    {
                        Status = g.Key,
                        Count = g.Count()
                    })
                    .ToList(),
                ByPriority = allWorkItems
                    .GroupBy(w => w.Priority)
                    .OrderBy(g => g.Key)
                    .Select(g => new WorkItemsCountByPriorityDto
                    {
                        Priority = g.Key,
                        Count = g.Count()
                    })
                    .ToList()
            };

            return Result<ProjectReportDto>.Success(report, HttpStatusCode.OK);
        }
    }
}
