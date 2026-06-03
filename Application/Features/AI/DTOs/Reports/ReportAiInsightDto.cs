using Domain.Enums;

namespace Application.Features.AI.DTOs.Reports;

public sealed class ReportAiInsightDto
{
    public ReportType ReportType { get; set; }

    public string Summary { get; set; } = string.Empty;

    public List<ReportAiInsightItemDto> KeyFindings { get; set; } = [];

    public List<ReportAiInsightItemDto> Risks { get; set; } = [];

    public List<ReportAiInsightItemDto> Recommendations { get; set; } = [];

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public sealed class ReportAiInsightItemDto
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}