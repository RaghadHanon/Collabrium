using Application.Common;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Reports;
using Application.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Application.Features.Reports.Queries;

public class GetProjectVelocityReportPdf
{
    public class Query : IRequest<Result<FileContentResult>>
    {
        public required string ProjectId { get; set; }
    }

    public class Handler(
        IReportsReadModel reportsReadModel,
        IProjectsRepository projectsRepository,
        ITenantContext tenantContext,
        IVelocityPdfRenderer velocityPdfRenderer
    ) : IRequestHandler<Query, Result<FileContentResult>>
    {
        public async Task<Result<FileContentResult>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.TenantId;
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return Result<FileContentResult>.Failure(
                    "Tenant not found.",
                    HttpStatusCode.BadRequest);
            }

            if (!await projectsRepository.IsProjectExists(tenantId, request.ProjectId, cancellationToken))
            {
                return Result<FileContentResult>.Failure(
                    "Project is not found.",
                    HttpStatusCode.BadRequest);
            }

            var report = await reportsReadModel.GenerateVelocityReport(
                tenantId,
                request.ProjectId);

            var pdfBytes = velocityPdfRenderer.Render(report);

            var fileResult = new FileContentResult(pdfBytes, "application/pdf")
            {
                FileDownloadName = $"VelocityReport_{report.ProjectKey}_{DateTime.UtcNow:yyyyMMddHHmm}.pdf"
            };

            return Result<FileContentResult>.Success(fileResult, HttpStatusCode.OK);
        }
    }
}