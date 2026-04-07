using Application.Common;
using Application.Features.Reports.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Reports.Queries;

public class GetProjectVelocityReport
{
    public class Query : IRequest<Result<VelocityReportDto>>
    {
        public required string ProjectId { get; set; }
    }

    public class Handler(
        IReportsReadModel reportsReadModel,
        IProjectsRepository projectsRepository,
        ITenantContext tenantContext
        ) : IRequestHandler<Query, Result<VelocityReportDto>>
    {
        public async Task<Result<VelocityReportDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.TenantId;
            if (string.IsNullOrEmpty(tenantId))
            {
                return Result<VelocityReportDto>.Failure("Tenant not found.", HttpStatusCode.BadRequest);
            }

            if (!await projectsRepository.IsProjectExists(tenantId, request.ProjectId, cancellationToken))
            {
                return Result<VelocityReportDto>.Failure("Project is not found.", HttpStatusCode.BadRequest);
            }

            var report = await reportsReadModel.GenerateVelocityReport(tenantId, request.ProjectId);

            return Result<VelocityReportDto>.Success(report, HttpStatusCode.OK);
        }
    }
}