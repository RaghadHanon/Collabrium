using Domain.Enums;

namespace Application.Features.Reports.DTOs;

public sealed class ProjectReportDto
{
    public required string ProjectId { get; set; }
    public required string ProjectName { get; set; }
    public DateTime GeneratedAtUtc { get; set; }

    public int TotalWorkItems { get; set; }
    public int BlockedWorkItems { get; set; }
    public int UnassignedWorkItems { get; set; }
    public int BacklogWorkItems { get; set; }

    public IReadOnlyList<WorkItemsCountByTypeDto> ByType { get; set; } = [];
    public IReadOnlyList<WorkItemsCountByStatusDto> ByStatus { get; set; } = [];
    public IReadOnlyList<WorkItemsCountByPriorityDto> ByPriority { get; set; } = [];
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
