namespace API.Contracts.Reports;

public class GetTeamsProductivityReportRequest : ReportDateRangeRequest
{
    public string? ProjectId { get; set; }
}
