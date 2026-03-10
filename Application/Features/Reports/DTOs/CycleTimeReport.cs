namespace Application.Features.Reports.DTOs;

public class CycleTimeReport
{
    public required string ProjectId { get; set; }
    public required string ProjectKey { get; set; }
    public DateTime GeneratedAt { get; set; }

    public double AverageCycleTimeDays { get; set; }
    public double MedianCycleTimeDays { get; set; }
    public double MinCycleTimeDays { get; set; }
    public double MaxCycleTimeDays { get; set; }

    public int TotalItemsAnalyzed { get; set; }

    public List<WorkItemCycleTime> Items { get; set; } = new List<WorkItemCycleTime>();
}

public class WorkItemCycleTime
{
    public required string WorkItemId { get; set; }
    public required string Code { get; set; }
    public DateTime EndDate { get; set; }
    public int CycleTimeDays { get; set; }
}