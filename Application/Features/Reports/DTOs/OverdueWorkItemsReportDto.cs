using Application.Features.Reports.DTOs.Shared;
using Domain.Enums;

namespace Application.Features.Reports.DTOs;

public class OverdueWorkItemsReportDto
{
    public string TenantId { get; set; } = null!;
    public string? ProjectId { get; set; }
    public string? TeamId { get; set; }
    public string? AssignedToUserId { get; set; }

    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public DateTime GeneratedAt { get; set; }

    public int TotalOverdueItems { get; set; }
    public int CriticalOverdueItems { get; set; }
    public int HighOverdueItems { get; set; }
    public int TotalOverdueEffortPoints { get; set; }
    public double AverageDaysOverdue { get; set; }

    public List<WorkItemPriorityDistributionDto> ByPriority { get; set; } = new();
    public List<WorkItemStatusDistributionDto> ByStatus { get; set; } = new();

    public List<OverdueWorkItemDto> Items { get; set; } = new();
}

public class OverdueWorkItemDto
{
    public string WorkItemId { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;

    public WorkItemType Type { get; set; }
    public WorkItemStatus Status { get; set; }
    public WorkItemPriority Priority { get; set; }

    public int EffortPoints { get; set; }
    public int DaysOverdue { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime EndDate { get; set; }

    public string ProjectId { get; set; } = null!;
    public string ProjectKey { get; set; } = null!;
    public string ProjectName { get; set; } = null!;

    public string? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
}