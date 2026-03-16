namespace Application.Features.Reports.DTOs.Summaries;

public class SprintsSummary
{
    public int TotalSprintsCount { get; set; }
    public int PlannedSprintsCount { get; set; }
    public int ActiveSprintsCount { get; set; }
    public int CompletedSprintsCount { get; set; }
    public int CancelledSprintsCount { get; set; }
}
