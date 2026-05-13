namespace Application.Features.Reports.DTOs;

public class TeamsProductivityReportDto
{
    public string TenantId { get; set; } = null!;
    public string? ProjectId { get; set; }

    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public DateTime GeneratedAt { get; set; }

    public int TeamsCount { get; set; }

    public int TotalAssignedWorkItems { get; set; }
    public int TotalCompletedWorkItems { get; set; }
    public int TotalBlockedWorkItems { get; set; }
    public int TotalOverdueWorkItems { get; set; }
    public int TotalCompletedEffortPoints { get; set; }

    public double AverageCompletionPercentage { get; set; }
    public double AverageCycleTimeDays { get; set; }

    public List<TeamProductivityItemDto> Teams { get; set; } = new();
}

public class TeamProductivityItemDto
{
    public string TeamId { get; set; } = null!;
    public string TeamName { get; set; } = null!;
    public string TeamLeadId { get; set; } = null!;
    public string TeamLeadName { get; set; } = null!;
    public string Department { get; set; } = null!;

    public int MembersCount { get; set; }
    public int? Capacity { get; set; }
    public int ActiveProjectsCount { get; set; }

    public int AssignedWorkItemsCount { get; set; }
    public int CompletedWorkItemsCount { get; set; }
    public int InProgressWorkItemsCount { get; set; }
    public int BlockedWorkItemsCount { get; set; }
    public int OverdueWorkItemsCount { get; set; }

    public int AssignedEffortPoints { get; set; }
    public int CompletedEffortPoints { get; set; }

    public double CompletionPercentage { get; set; }
    public double EffortCompletionPercentage { get; set; }
    public double AverageCycleTimeDays { get; set; }
}