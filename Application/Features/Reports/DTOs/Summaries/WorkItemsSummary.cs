namespace Application.Features.Reports.DTOs.Summaries;

public class WorkItemsSummary
{
    public int TotalWorkItemsCount { get; set; }
    public int EpicsCount { get; set; }
    public int UserStoriesCount { get; set; }
    public int TaskItemCount { get; set; }
    public int BugsCount { get; set; }
    public int TestCasesCount { get; set; }
}
