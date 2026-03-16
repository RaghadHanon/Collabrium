using Application.Common;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Infrastructure.Pdf;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Application.Features.Reports.Queries;

public class GetCycleTimeReportPdf
{
    public class Query : IRequest<Result<FileContentResult>>
    {
        public string? TenantId { get; set; }
        public required string ProjectId { get; set; }
        public string? SprintId { get; set; }
        public DateTime From { get; set; } = DateTime.UtcNow.AddDays(-14);
        public DateTime To { get; set; } = DateTime.UtcNow;
    }

    public class Handler(
        IReportsReadModel reportsReadModel,
        IProjectsRepository projectsRepository,
        ISprintsRepository sprintsRepository,
        ITenantContext tenantContext,
        IChartService chartService,
        IPdfService pdfService
    ) : IRequestHandler<Query, Result<FileContentResult>>
    {
        public async Task<Result<FileContentResult>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Resolve TenantId
            if (request.TenantId is null)
            {
                request.TenantId = tenantContext.TenantId;
                if (request.TenantId is null)
                    return Result<FileContentResult>.Failure("TenantId is not found.", HttpStatusCode.BadRequest);
            }

            // Check Project existence
            if (!await projectsRepository.IsProjectExists(request.TenantId, request.ProjectId))
                return Result<FileContentResult >.Failure($"Project with id {request.ProjectId} does not exist.", HttpStatusCode.NotFound);

            // Check Sprint existence if provided
            if (request.SprintId != null)
            {
                if (!await sprintsRepository.IsExistsAsync(request.TenantId, request.SprintId, request.ProjectId))
                    return Result<FileContentResult>.Failure($"Sprint with id {request.SprintId} does not exist in project {request.ProjectId}.", HttpStatusCode.NotFound);
            }

            // Generate the report
            var report = await reportsReadModel.GenerateCycleTimeReportAsync(
                request.TenantId,
                request.ProjectId,
                request.From,
                request.To,
                request.SprintId
            );

            // Generate charts
            var barChart = chartService.CreateBarChart(report);
            var distributionChart = chartService.CreateDistributionChart(report);
            var summaryChart = chartService.CreateSummaryChart(report);

            // Generate PDF
            var pdfBytes = pdfService.GeneratePdf(report, barChart, distributionChart, summaryChart);

            var fileResult = new FileContentResult(pdfBytes, "application/pdf")
            {
                FileDownloadName = $"CycleTimeReport_{report.ProjectKey}_{DateTime.UtcNow:yyyyMMddHHmm}.pdf"
            };

            return Result<FileContentResult>.Success(fileResult, HttpStatusCode.OK);
        }
    }
}