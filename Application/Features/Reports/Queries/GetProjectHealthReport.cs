using Application.Common;
using Application.Features.Reports.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Reports.Queries;

public class GetProjectHealthReport
{
    public class Query : IRequest<Result<ProjectHealthReportDto>>
    {
        public string? TenantId { get; set; }
        public string? ProjectId { get; set; }
        public DateTime From { get; set; } = DateTime.UtcNow.AddDays(-30);
        public DateTime To { get; set; } = DateTime.UtcNow;
    }

    public class Handler(
        IReportsReadModel reportsReadModel,
        IProjectsRepository projectsRepository,
        ITenantContext tenantContext
    ) : IRequestHandler<Query, Result<ProjectHealthReportDto>>
    {
        public async Task<Result<ProjectHealthReportDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (request.TenantId is null)
            {
                request.TenantId = tenantContext.TenantId;

                if (request.TenantId is null)
                {
                    return Result<ProjectHealthReportDto>.Failure("TenantId is not found.", HttpStatusCode.BadRequest);
                }
            }

            if (request.From > request.To)
            {
                return Result<ProjectHealthReportDto>.Failure("From date cannot be after To date.", HttpStatusCode.BadRequest);
            }

            if (request.ProjectId is not null)
            {
                if (!await projectsRepository.IsProjectExists(request.TenantId, request.ProjectId))
                {
                    return Result<ProjectHealthReportDto>.Failure(
                        $"Project with id {request.ProjectId} does not exist.",
                        HttpStatusCode.NotFound);
                }
            }

            var report = await reportsReadModel.GenerateProjectHealthReportAsync(
                request.TenantId,
                request.From,
                request.To,
                request.ProjectId);

            return Result<ProjectHealthReportDto>.Success(report, HttpStatusCode.OK);
        }
    }
}