using Domain.Enums;

namespace Application.Features.Reports.DTOs;

public class BugSeverityDistributionReportDto
{
    public string TenantId { get; set; } = null!;
    public string? ProjectId { get; set; }
    public string? ProjectKey { get; set; }
    public string? ProjectName { get; set; }

    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public DateTime GeneratedAt { get; set; }

    public int TotalBugs { get; set; }

    public int LowBugsCount { get; set; }
    public int MediumBugsCount { get; set; }
    public int HighBugsCount { get; set; }
    public int CriticalBugsCount { get; set; }

    public double LowPercentage { get; set; }
    public double MediumPercentage { get; set; }
    public double HighPercentage { get; set; }
    public double CriticalPercentage { get; set; }

    public List<BugSeverityDistributionItemDto> Items { get; set; } = new();
}

public class BugSeverityDistributionItemDto
{
    public WorkItemPriority Severity { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}