using Application.Features.AI.DTOs.Reports;
using Application.Features.AI.Queries;

namespace Application.Interfaces.AI;

public interface IReportAiInsightService
{
    Task<ReportAiInsightDto> GenerateAsync(
        GenerateReportAiInsight.Query query,
        CancellationToken cancellationToken = default);
}