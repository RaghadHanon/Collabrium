namespace API.Contracts.Reports;

public class GetCycleTimeReportRequest : ReportDateRangeRequest
{
    public string? SprintId { get; set; }
}