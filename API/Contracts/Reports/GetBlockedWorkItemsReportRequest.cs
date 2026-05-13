namespace API.Contracts.Reports;

public class GetBlockedWorkItemsReportRequest : ReportDateRangeRequest
{
    public string? TeamId { get; set; }
    public string? AssignedToUserId { get; set; }
    public int MaxItems { get; set; } = 100;
}
