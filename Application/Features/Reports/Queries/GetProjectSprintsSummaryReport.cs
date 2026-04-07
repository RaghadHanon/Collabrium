using Application.Common;
using Application.Features.Reports.DTOs.Summaries;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Reports.Queries;

public class GetProjectSprintsSummaryReport
{
    public class Query : IRequest<Result<SprintsSummary>>
    {
        public required string ProjectId { get; set; }
    }

    public class Handler(
        IReportsReadModel reportsReadModel,
        IProjectsRepository projectsRepository,
        ITenantContext tenantContext
        ) : IRequestHandler<Query, Result<SprintsSummary>>
    {
        public async Task<Result<SprintsSummary>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.TenantId;
            if (string.IsNullOrEmpty(tenantId))
            {
                return Result<SprintsSummary>.Failure("Tenant not found.", HttpStatusCode.BadRequest);
            }

            if (!await projectsRepository.IsProjectExists(tenantId, request.ProjectId, cancellationToken))
            {
                return Result<SprintsSummary>.Failure("Project is not found.", HttpStatusCode.BadRequest);
            }

            var report = await reportsReadModel.GenerateProjectSprintsSummary(tenantId, request.ProjectId);

            return Result<SprintsSummary>.Success(report, HttpStatusCode.OK);
        }
    }
}