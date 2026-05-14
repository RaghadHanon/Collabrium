namespace API.Contracts.Reports;

public class GetDeveloperProductivityReportRequest : ReportDateRangeRequest
{
    public string? ProjectId { get; set; }
    public string? TeamId { get; set; }
    public string? UserId { get; set; }
}