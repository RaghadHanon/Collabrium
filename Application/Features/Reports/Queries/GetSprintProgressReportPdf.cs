using Application.Common;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Reports;
using Application.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Application.Features.Reports.Queries;

public class GetSprintProgressReportPdf
{
    public class Query : IRequest<Result<FileContentResult>>
    {
        public required string ProjectId { get; set; }
        public required string SprintId { get; set; }
    }

    public class Handler(
        IReportsReadModel reportsReadModel,
        IProjectsRepository projectsRepository,
        ISprintsRepository sprintsRepository,
        ITenantContext tenantContext,
        ISprintProgressPdfRenderer sprintProgressPdfRenderer
    ) : IRequestHandler<Query, Result<FileContentResult>>
    {
        public async Task<Result<FileContentResult>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.TenantId;
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return Result<FileContentResult>.Failure("Tenant not found.", HttpStatusCode.BadRequest);
            }

            if (!await projectsRepository.IsProjectExists(tenantId, request.ProjectId, cancellationToken))
            {
                return Result<FileContentResult>.Failure("Project is not found.", HttpStatusCode.BadRequest);
            }

            if (!await sprintsRepository.IsExistsAsync(tenantId, request.SprintId, request.ProjectId))
            {
                return Result<FileContentResult>.Failure("Sprint is not found.", HttpStatusCode.BadRequest);
            }

            var report = await reportsReadModel.GenerateSprintProgressReport(
                tenantId,
                request.ProjectId,
                request.SprintId);

            var pdfBytes = sprintProgressPdfRenderer.Render(report);

            var fileResult = new FileContentResult(pdfBytes, "application/pdf")
            {
                FileDownloadName = $"SprintProgressReport_{request.SprintId}_{DateTime.UtcNow:yyyyMMddHHmm}.pdf"
            };

            return Result<FileContentResult>.Success(fileResult, HttpStatusCode.OK);
        }
    }
}