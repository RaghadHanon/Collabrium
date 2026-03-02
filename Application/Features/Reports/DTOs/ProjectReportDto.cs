using Domain.Enums;

namespace Application.Features.Reports.DTOs;

public sealed class ProjectReportDto
{
    public required string ProjectId { get; set; }
    public required string ProjectKey { get; set; }
    public required string ProjectName { get; set; }
    public DateTime GeneratedAtUtc { get; set; }

    public required ReportWindowDto Window { get; set; }
    public required ProjectOverviewDto Overview { get; set; }
    public required ThroughputDto Throughput { get; set; }

    public IReadOnlyList<WorkItemsCountByTypeDto> ByType { get; set; } = [];
    public IReadOnlyList<WorkItemsCountByStatusDto> ByStatus { get; set; } = [];
    public IReadOnlyList<WorkItemsCountByPriorityDto> ByPriority { get; set; } = [];

    public IReadOnlyList<SprintHealthDto> SprintHealth { get; set; } = [];
    public IReadOnlyList<AssigneeWorkloadDto> AssigneeWorkload { get; set; } = [];
    public required AgingDto Aging { get; set; }

    public IReadOnlyList<string> RiskFlags { get; set; } = [];
    public IReadOnlyList<string> Recommendations { get; set; } = [];
}

public sealed class ReportWindowDto
{
    public DateTime? StartDateUtc { get; set; }
    public DateTime? EndDateUtc { get; set; }
}

public sealed class ProjectOverviewDto
{
    public int TotalWorkItems { get; set; }
    public int DoneWorkItems { get; set; }
    public int InProgressWorkItems { get; set; }
    public int BlockedWorkItems { get; set; }
    public int BacklogWorkItems { get; set; }
    public int UnassignedWorkItems { get; set; }

    public decimal CompletionRatePercent { get; set; }
    public decimal BlockedRatePercent { get; set; }
}

public sealed class ThroughputDto
{
    public int CreatedInWindow { get; set; }
    public int CompletedInWindow { get; set; }
    public int NetFlow { get; set; }
}

public sealed class SprintHealthDto
{
    public required string SprintId { get; set; }
    public required string SprintName { get; set; }
    public SprintStatus SprintStatus { get; set; }

    public int ScopeCount { get; set; }
    public int DoneCount { get; set; }
    public decimal CompletionPercent { get; set; }
}

public sealed class AssigneeWorkloadDto
{
    public string? UserId { get; set; }
    public required string DisplayName { get; set; }

    public int TotalAssigned { get; set; }
    public int InProgressCount { get; set; }
    public int BlockedCount { get; set; }
    public int DoneCount { get; set; }
}

public sealed class AgingDto
{
    public int Fresh0To7Days { get; set; }
    public int Warm8To30Days { get; set; }
    public int Old31To60Days { get; set; }
    public int Stale60PlusDays { get; set; }
}

public sealed class WorkItemsCountByTypeDto
{
    public WorkItemType Type { get; set; }
    public int Count { get; set; }
}

public sealed class WorkItemsCountByStatusDto
{
    public WorkItemStatus Status { get; set; }
    public int Count { get; set; }
}

public sealed class WorkItemsCountByPriorityDto
{
    public WorkItemPriority Priority { get; set; }
    public int Count { get; set; }
}
