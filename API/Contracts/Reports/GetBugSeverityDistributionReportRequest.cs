namespace API.Contracts.Reports;

public class GetBugSeverityDistributionReportRequest : ReportDateRangeRequest
{
    public string? ProjectId { get; set; }
}
