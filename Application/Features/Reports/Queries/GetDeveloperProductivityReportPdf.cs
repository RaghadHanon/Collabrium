using Application.Common;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Reports;
using Application.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Application.Features.Reports.Queries;

public class GetDeveloperProductivityReportPdf
{
    public class Query : IRequest<Result<FileContentResult>>
    {
        public string? TenantId { get; set; }
        public string? ProjectId { get; set; }
        public string? TeamId { get; set; }
        public DateTime From { get; set; } = DateTime.UtcNow.AddDays(-30);
        public DateTime To { get; set; } = DateTime.UtcNow;
    }

    public class Handler(
        IReportsReadModel reportsReadModel,
        IProjectsRepository projectsRepository,
        ITeamsRepository teamsRepository,
        ITenantContext tenantContext,
        IDeveloperProductivityPdfRenderer developerProductivityPdfRenderer
    ) : IRequestHandler<Query, Result<FileContentResult>>
    {
        public async Task<Result<FileContentResult>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (request.TenantId is null)
            {
                request.TenantId = tenantContext.TenantId;

                if (request.TenantId is null)
                    return Result<FileContentResult>.Failure(
                        "TenantId is not found.",
                        HttpStatusCode.BadRequest);
            }

            if (request.From > request.To)
            {
                return Result<FileContentResult>.Failure(
                    "From date cannot be after To date.",
                    HttpStatusCode.BadRequest);
            }

            if (request.ProjectId is not null)
            {
                if (!await projectsRepository.IsProjectExists(request.TenantId, request.ProjectId))
                {
                    return Result<FileContentResult>.Failure(
                        $"Project with id {request.ProjectId} does not exist.",
                        HttpStatusCode.NotFound);
                }
            }

            if (request.TeamId is not null)
            {
                if (!await teamsRepository.IsTeamExists(request.TenantId, request.TeamId))
                {
                    return Result<FileContentResult>.Failure(
                        $"Team with id {request.TeamId} does not exist.",
                        HttpStatusCode.NotFound);
                }
            }

            var report = await reportsReadModel.GenerateDeveloperProductivityReportAsync(
                request.TenantId,
                request.From,
                request.To,
                request.ProjectId,
                request.TeamId);

            var pdfBytes = developerProductivityPdfRenderer.Render(report);

            var projectPart = report.ProjectKey ?? "AllProjects";
            var teamPart = report.TeamId ?? "AllTeams";

            var fileResult = new FileContentResult(pdfBytes, "application/pdf")
            {
                FileDownloadName = $"DeveloperProductivityReport_{projectPart}_{teamPart}_{DateTime.UtcNow:yyyyMMddHHmm}.pdf"
            };

            return Result<FileContentResult>.Success(fileResult, HttpStatusCode.OK);
        }
    }
}