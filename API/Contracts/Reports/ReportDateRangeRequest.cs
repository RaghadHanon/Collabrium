namespace API.Contracts.Reports;

public class ReportDateRangeRequest
{
    public DateTime From { get; set; } = DateTime.UtcNow.AddDays(-30);
    public DateTime To { get; set; } = DateTime.UtcNow;
}
