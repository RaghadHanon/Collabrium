using Application.Common;
using Application.Features.Reports.DTOs;
using Application.Features.Sprints.DTOs;
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
        public DateTime? StartDateUtc { get; set; }
        public DateTime? EndDateUtc { get; set; }
    }

    public sealed class Handler(
        ITenantContext tenantContext,
        IProjectsReadModel projectsReadModel,
        IWorkItemsReadModel workItemsReadModel,
        ISprintsReadModel sprintsReadModel,
        IUsersReadModel usersReadModel) : IRequestHandler<Query, Result<ProjectReportDto>>
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

            var windowEnd = request.EndDateUtc ?? DateTime.UtcNow;
            var windowStart = request.StartDateUtc ?? windowEnd.AddDays(-30);
            if (windowStart > windowEnd)
                return Result<ProjectReportDto>.Failure("StartDateUtc must be before EndDateUtc.", HttpStatusCode.BadRequest);

            var allWorkItems = await FetchAllWorkItems(tenantId, request.ProjectId, workItemsReadModel, cancellationToken);
            var allSprints = await FetchAllSprints(tenantId, request.ProjectId, sprintsReadModel, cancellationToken);

            var doneItems = allWorkItems.Where(w => w.Status == Domain.Enums.WorkItemStatus.Done).ToList();
            var inProgressItems = allWorkItems.Where(w => w.Status == Domain.Enums.WorkItemStatus.InProgress).ToList();

            var createdInWindow = allWorkItems.Count(w => w.CreatedAt >= windowStart && w.CreatedAt <= windowEnd);
            var completedInWindow = await CountCompletedInWindow(
                tenantId,
                request.ProjectId,
                doneItems,
                windowStart,
                windowEnd,
                workItemsReadModel,
                cancellationToken);

            var byType = allWorkItems
                .GroupBy(w => w.Type)
                .OrderBy(g => g.Key)
                .Select(g => new WorkItemsCountByTypeDto { Type = g.Key, Count = g.Count() })
                .ToList();

            var byStatus = allWorkItems
                .GroupBy(w => w.Status)
                .OrderBy(g => g.Key)
                .Select(g => new WorkItemsCountByStatusDto { Status = g.Key, Count = g.Count() })
                .ToList();

            var byPriority = allWorkItems
                .GroupBy(w => w.Priority)
                .OrderBy(g => g.Key)
                .Select(g => new WorkItemsCountByPriorityDto { Priority = g.Key, Count = g.Count() })
                .ToList();

            var sprintHealth = allSprints
                .Select(s =>
                {
                    var scope = allWorkItems.Where(w => w.SprintId == s.Id).ToList();
                    var done = scope.Count(w => w.Status == Domain.Enums.WorkItemStatus.Done);
                    var completion = scope.Count == 0 ? 0m : Math.Round((decimal)done / scope.Count * 100m, 2);

                    return new SprintHealthDto
                    {
                        SprintId = s.Id,
                        SprintName = s.Name,
                        SprintStatus = s.Status,
                        ScopeCount = scope.Count,
                        DoneCount = done,
                        CompletionPercent = completion
                    };
                })
                .OrderByDescending(x => x.CompletionPercent)
                .ThenBy(x => x.SprintName)
                .ToList();

            var assigneeWorkload = await BuildAssigneeWorkload(allWorkItems, usersReadModel, cancellationToken);

            var aging = BuildAging(allWorkItems, windowEnd);
            var overview = BuildOverview(allWorkItems, doneItems.Count, inProgressItems.Count);

            var report = new ProjectReportDto
            {
                ProjectId = project.Id,
                ProjectKey = project.Key,
                ProjectName = project.Name,
                GeneratedAtUtc = DateTime.UtcNow,
                Window = new ReportWindowDto
                {
                    StartDateUtc = windowStart,
                    EndDateUtc = windowEnd
                },
                Overview = overview,
                Throughput = new ThroughputDto
                {
                    CreatedInWindow = createdInWindow,
                    CompletedInWindow = completedInWindow,
                    NetFlow = completedInWindow - createdInWindow
                },
                ByType = byType,
                ByStatus = byStatus,
                ByPriority = byPriority,
                SprintHealth = sprintHealth,
                AssigneeWorkload = assigneeWorkload,
                Aging = aging,
                RiskFlags = BuildRiskFlags(overview, sprintHealth, aging),
                Recommendations = BuildRecommendations(overview, sprintHealth, assigneeWorkload, aging)
            };

            return Result<ProjectReportDto>.Success(report, HttpStatusCode.OK);
        }


        private static async Task<int> CountCompletedInWindow(
            string tenantId,
            string projectId,
            IReadOnlyCollection<WorkItemListItemDto> doneItems,
            DateTime windowStart,
            DateTime windowEnd,
            IWorkItemsReadModel workItemsReadModel,
            CancellationToken cancellationToken)
        {
            var count = 0;
            foreach (var item in doneItems)
            {
                var details = await workItemsReadModel.GetByIdAsync(tenantId, projectId, item.Id, cancellationToken);
                if (details?.EndDate is DateTime completedAt
                    && completedAt >= windowStart
                    && completedAt <= windowEnd)
                {
                    count++;
                }
            }

            return count;
        }

        private static async Task<List<WorkItemListItemDto>> FetchAllWorkItems(
            string tenantId,
            string projectId,
            IWorkItemsReadModel workItemsReadModel,
            CancellationToken cancellationToken)
        {
            var allWorkItems = new List<WorkItemListItemDto>();
            DateTime? cursor = null;

            do
            {
                var page = await workItemsReadModel.GetWorkItemsAsync(new WorkItemsSearchAndFilterParametersDto
                {
                    TenantId = tenantId,
                    ProjectId = projectId,
                    Cursor = cursor,
                    PageSize = MaxPageSize
                }, cancellationToken);

                allWorkItems.AddRange(page.Items);
                cursor = page.NextCursor;
            }
            while (cursor.HasValue);

            return allWorkItems;
        }

        private static async Task<List<SprintDto>> FetchAllSprints(
            string tenantId,
            string projectId,
            ISprintsReadModel sprintsReadModel,
            CancellationToken cancellationToken)
        {
            var allSprints = new List<SprintDto>();
            DateTime? cursor = null;

            do
            {
                var page = await sprintsReadModel.GetSprintsAsync(new SprintsSearchAndFilterParametersDto
                {
                    TenantId = tenantId,
                    ProjectId = projectId,
                    Cursor = cursor,
                    PageSize = MaxPageSize
                }, cancellationToken);

                allSprints.AddRange(page.Items);
                cursor = page.NextCursor;
            }
            while (cursor.HasValue);

            return allSprints;
        }

        private static ProjectOverviewDto BuildOverview(IReadOnlyCollection<WorkItemListItemDto> allWorkItems, int doneCount, int inProgressCount)
        {
            var total = allWorkItems.Count;
            var blocked = allWorkItems.Count(w => w.IsBlocked);

            return new ProjectOverviewDto
            {
                TotalWorkItems = total,
                DoneWorkItems = doneCount,
                InProgressWorkItems = inProgressCount,
                BlockedWorkItems = blocked,
                BacklogWorkItems = allWorkItems.Count(w => string.IsNullOrWhiteSpace(w.SprintId)),
                UnassignedWorkItems = allWorkItems.Count(w => string.IsNullOrWhiteSpace(w.AssignedToUserId)),
                CompletionRatePercent = total == 0 ? 0 : Math.Round((decimal)doneCount / total * 100m, 2),
                BlockedRatePercent = total == 0 ? 0 : Math.Round((decimal)blocked / total * 100m, 2)
            };
        }

        private static AgingDto BuildAging(IReadOnlyCollection<WorkItemListItemDto> allWorkItems, DateTime referenceDateUtc)
        {
            var openItems = allWorkItems.Where(w => w.Status != Domain.Enums.WorkItemStatus.Done).ToList();
            var fresh = 0;
            var warm = 0;
            var old = 0;
            var stale = 0;

            foreach (var item in openItems)
            {
                var age = (referenceDateUtc - item.CreatedAt).TotalDays;
                if (age <= 7)
                    fresh++;
                else if (age <= 30)
                    warm++;
                else if (age <= 60)
                    old++;
                else
                    stale++;
            }

            return new AgingDto
            {
                Fresh0To7Days = fresh,
                Warm8To30Days = warm,
                Old31To60Days = old,
                Stale60PlusDays = stale
            };
        }

        private static List<string> BuildRiskFlags(ProjectOverviewDto overview, IReadOnlyCollection<SprintHealthDto> sprintHealth, AgingDto aging)
        {
            var riskFlags = new List<string>();

            if (overview.BlockedRatePercent >= 15)
                riskFlags.Add("High blocked ratio: at least 15% of current scope is blocked.");

            if (overview.UnassignedWorkItems >= 10)
                riskFlags.Add("Unassigned queue is high: 10+ work items are currently ownerless.");

            if (aging.Stale60PlusDays > 0)
                riskFlags.Add("Stale work detected: one or more open items are older than 60 days.");

            if (sprintHealth.Any() && sprintHealth.Count(s => s.CompletionPercent < 50) >= 2)
                riskFlags.Add("Delivery risk: multiple sprints have completion below 50%.");

            return riskFlags;
        }

        private static List<string> BuildRecommendations(
            ProjectOverviewDto overview,
            IReadOnlyCollection<SprintHealthDto> sprintHealth,
            IReadOnlyCollection<AssigneeWorkloadDto> assigneeWorkload,
            AgingDto aging)
        {
            var recs = new List<string>();

            if (overview.BlockedWorkItems > 0)
                recs.Add("Run a blocker triage meeting twice a week and assign explicit unblock owners.");

            if (overview.UnassignedWorkItems > 0)
                recs.Add("Reduce ownerless work by assigning every in-flight item to a single accountable assignee.");

            if (sprintHealth.Any(s => s.CompletionPercent < 60))
                recs.Add("Rebalance sprint scope: split oversized issues and cap carry-over to improve predictability.");

            if (assigneeWorkload.Any())
            {
                var maxAssigned = assigneeWorkload.Max(x => x.TotalAssigned);
                var minAssigned = assigneeWorkload.Min(x => x.TotalAssigned);
                if (maxAssigned - minAssigned >= 7)
                    recs.Add("Workload is uneven across assignees; rebalance assignments to prevent bottlenecks.");
            }

            if (aging.Stale60PlusDays > 0)
                recs.Add("Introduce aging SLA alerts for open items older than 30/60 days.");

            if (!recs.Any())
                recs.Add("Current flow is healthy. Keep monitoring blocked ratio, completion trend, and stale backlog weekly.");

            return recs;
        }

        private static async Task<List<AssigneeWorkloadDto>> BuildAssigneeWorkload(
            IReadOnlyCollection<WorkItemListItemDto> allWorkItems,
            IUsersReadModel usersReadModel,
            CancellationToken cancellationToken)
        {
            var grouped = allWorkItems
                .GroupBy(w => w.AssignedToUserId)
                .OrderByDescending(g => g.Count())
                .ToList();

            var results = new List<AssigneeWorkloadDto>(grouped.Count);
            foreach (var group in grouped)
            {
                var userId = group.Key;
                string displayName;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    displayName = "Unassigned";
                }
                else
                {
                    var user = await usersReadModel.GetUserByIdAsync(userId, cancellationToken);
                    displayName = user?.FullName ?? $"User {userId}";
                }

                results.Add(new AssigneeWorkloadDto
                {
                    UserId = userId,
                    DisplayName = displayName,
                    TotalAssigned = group.Count(),
                    InProgressCount = group.Count(x => x.Status == Domain.Enums.WorkItemStatus.InProgress),
                    BlockedCount = group.Count(x => x.IsBlocked),
                    DoneCount = group.Count(x => x.Status == Domain.Enums.WorkItemStatus.Done)
                });
            }

            return results;
        }
    }
}
