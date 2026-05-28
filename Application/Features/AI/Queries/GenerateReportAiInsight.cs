using Application.Common;
using Application.Features.AI.DTOs.Reports;
using Application.Interfaces.AI;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.AI.Queries;

public class GenerateReportAiInsight
{
    public class Query : IRequest<Result<ReportAiInsightDto>>
    {
        public ReportType ReportType { get; set; }

        public string? ProjectId { get; set; }
        public string? SprintId { get; set; }
        public string? TeamId { get; set; }
        public string? UserId { get; set; }
        public string? AssignedToUserId { get; set; }

        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public int? MaxItems { get; set; }
    }

    public class Handler(IReportAiInsightService reportAiInsightService)
        : IRequestHandler<Query, Result<ReportAiInsightDto>>
    {
        public async Task<Result<ReportAiInsightDto>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var insight = await reportAiInsightService.GenerateAsync(
                request,
                cancellationToken);

            return Result<ReportAiInsightDto>.Success(
                insight,
                HttpStatusCode.OK);
        }
    }
}