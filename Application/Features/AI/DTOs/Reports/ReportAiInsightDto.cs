using Domain.Enums;

namespace Application.Features.AI.DTOs.Reports;

public sealed class ReportAiInsightDto
{
    public ReportType ReportType { get; set; }
    public string Insight { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}