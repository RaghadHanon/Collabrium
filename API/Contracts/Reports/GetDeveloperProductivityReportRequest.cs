namespace API.Contracts.Reports;

public class GetDeveloperProductivityReportRequest : ReportDateRangeRequest
{
    public string? TeamId { get; set; }
}
