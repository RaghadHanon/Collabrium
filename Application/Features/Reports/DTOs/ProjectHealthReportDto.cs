using Domain.Enums;

namespace Application.Features.Reports.DTOs;

public class ProjectHealthReportDto
{
    public string TenantId { get; set; } = null!;
    public string? ProjectId { get; set; }

    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public DateTime GeneratedAt { get; set; }

    public int ProjectsCount { get; set; }
    public int HealthyProjectsCount { get; set; }
    public int AtRiskProjectsCount { get; set; }
    public int CriticalProjectsCount { get; set; }

    public double AverageHealthScore { get; set; }

    public List<ProjectHealthItemDto> Projects { get; set; } = new();
}

public class ProjectHealthItemDto
{
    public string ProjectId { get; set; } = null!;
    public string ProjectKey { get; set; } = null!;
    public string ProjectName { get; set; } = null!;
    public ProjectStatus ProjectStatus { get; set; }

    public string? TeamId { get; set; }
    public string? TeamName { get; set; }

    public int TotalWorkItems { get; set; }
    public int CompletedWorkItems { get; set; }
    public int BlockedWorkItems { get; set; }
    public int OverdueWorkItems { get; set; }

    public int TotalBugs { get; set; }
    public int CriticalBugs { get; set; }
    public int HighBugs { get; set; }

    public int TotalEffortPoints { get; set; }
    public int CompletedEffortPoints { get; set; }

    public double CompletionPercentage { get; set; }
    public double EffortCompletionPercentage { get; set; }
    public double BlockedPercentage { get; set; }
    public double OverduePercentage { get; set; }

    public double HealthScore { get; set; }
    public string HealthStatus { get; set; } = null!;
}