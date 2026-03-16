namespace API.Contracts.Reports;

public class GetCycleTimeReportRequest
{
    public string? SprintId { get; set; }
    public DateTime? From { get; set; } = DateTime.UtcNow.AddDays(-14);
    public DateTime? To { get; set; } = DateTime.UtcNow;
}
