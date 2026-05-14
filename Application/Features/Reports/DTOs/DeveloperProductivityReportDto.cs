namespace Application.Features.Reports.DTOs;

public class DeveloperProductivityReportDto
{
    public string TenantId { get; set; } = null!;
    public string? ProjectId { get; set; }
    public string? ProjectKey { get; set; }
    public string? ProjectName { get; set; }
    public string? TeamId { get; set; }
    public string? UserId { get; set; }

    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public DateTime GeneratedAt { get; set; }

    public int DevelopersCount { get; set; }

    public int TotalAssignedWorkItems { get; set; }
    public int TotalCompletedWorkItems { get; set; }
    public int TotalBlockedWorkItems { get; set; }
    public int TotalOverdueWorkItems { get; set; }
    public int TotalCompletedEffortPoints { get; set; }

    public double AverageCompletionPercentage { get; set; }
    public double AverageCycleTimeDays { get; set; }

    public List<DeveloperProductivityItemDto> Developers { get; set; } = new();
}

public class DeveloperProductivityItemDto
{
    public string UserId { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string JobTitle { get; set; } = null!;
    public string? Department { get; set; }

    public int AssignedWorkItemsCount { get; set; }
    public int CreatedWorkItemsCount { get; set; }
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