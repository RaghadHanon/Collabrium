namespace API.Contracts.Reports;

public class GetProjectHealthReportRequest : ReportDateRangeRequest
{
    public string? ProjectId { get; set; }
}