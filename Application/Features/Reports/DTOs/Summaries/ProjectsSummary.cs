namespace Application.Features.Reports.DTOs.Summaries;

public class ProjectsSummary
{
    public int TotalProjectsCount { get; set; }
    public int NotStartedProjectsCount { get; set; }
    public int InProgressProjectsCount { get; set; }
    public int CompletedProjectsCount { get; set; }
    public int OnHoldProjectsCount { get; set; }
    public int CancelledProjectsCount { get; set; }
}
