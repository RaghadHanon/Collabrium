namespace Application.Features.AI.DTOs.Reports;

public sealed class ReportAiInsightFilters
{
    public string? TenantId { get; set; }

    public string? ProjectId { get; set; }
    public string? SprintId { get; set; }
    public string? TeamId { get; set; }
    public string? UserId { get; set; }
    public string? AssignedToUserId { get; set; }

    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    public int? MaxItems { get; set; }
}