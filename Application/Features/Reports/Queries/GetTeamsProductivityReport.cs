using Application.Common;
using Application.Features.Reports.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Reports.Queries;

public class GetTeamsProductivityReport
{
    public class Query : IRequest<Result<TeamsProductivityReportDto>>
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
    ) : IRequestHandler<Query, Result<TeamsProductivityReportDto>>
    {
        public async Task<Result<TeamsProductivityReportDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (request.TenantId is null)
            {
                request.TenantId = tenantContext.TenantId;

                if (request.TenantId is null)
                {
                    return Result<TeamsProductivityReportDto>.Failure("TenantId is not found.", HttpStatusCode.BadRequest);
                }
            }

            if (request.From > request.To)
            {
                return Result<TeamsProductivityReportDto>.Failure("From date cannot be after To date.", HttpStatusCode.BadRequest);
            }

            if (request.ProjectId is not null)
            {
                if (!await projectsRepository.IsProjectExists(request.TenantId, request.ProjectId))
                {
                    return Result<TeamsProductivityReportDto>.Failure(
                        $"Project with id {request.ProjectId} does not exist.",
                        HttpStatusCode.NotFound);
                }
            }

            var report = await reportsReadModel.GenerateTeamsProductivityReportAsync(
                request.TenantId,
                request.From,
                request.To,
                request.ProjectId);

            return Result<TeamsProductivityReportDto>.Success(report, HttpStatusCode.OK);
        }
    }
}