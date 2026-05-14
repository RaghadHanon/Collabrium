using Application.Common;
using Application.Features.Reports.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Reports.Queries;

public class GetDeveloperProductivityReport
{
    public class Query : IRequest<Result<DeveloperProductivityReportDto>>
    {
        public string? TenantId { get; set; }
        public string? ProjectId { get; set; }
        public string? TeamId { get; set; }
        public string? UserId { get; set; }
        public DateTime From { get; set; } = DateTime.UtcNow.AddDays(-30);
        public DateTime To { get; set; } = DateTime.UtcNow;
    }

    public class Handler(
        IReportsReadModel reportsReadModel,
        IProjectsRepository projectsRepository,
        ITeamsRepository teamsRepository,
        IUsersRepository usersRepository,
        ITenantContext tenantContext
    ) : IRequestHandler<Query, Result<DeveloperProductivityReportDto>>
    {
        public async Task<Result<DeveloperProductivityReportDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (request.TenantId is null)
            {
                request.TenantId = tenantContext.TenantId;

                if (request.TenantId is null)
                {
                    return Result<DeveloperProductivityReportDto>.Failure(
                        "TenantId is not found.",
                        HttpStatusCode.BadRequest);
                }
            }

            if (request.From > request.To)
            {
                return Result<DeveloperProductivityReportDto>.Failure(
                    "From date cannot be after To date.",
                    HttpStatusCode.BadRequest);
            }

            if (request.TeamId is not null && request.UserId is not null)
            {
                return Result<DeveloperProductivityReportDto>.Failure(
                    "Choose either TeamId or UserId, not both.",
                    HttpStatusCode.BadRequest);
            }

            if (request.ProjectId is not null)
            {
                if (!await projectsRepository.IsProjectExists(request.TenantId, request.ProjectId))
                {
                    return Result<DeveloperProductivityReportDto>.Failure(
                        $"Project with id {request.ProjectId} does not exist.",
                        HttpStatusCode.NotFound);
                }
            }

            if (request.TeamId is not null)
            {
                if (!await teamsRepository.IsTeamExists(request.TenantId, request.TeamId))
                {
                    return Result<DeveloperProductivityReportDto>.Failure(
                        $"Team with id {request.TeamId} does not exist.",
                        HttpStatusCode.NotFound);
                }
            }

            if (request.UserId is not null)
            {
                if (!await usersRepository.IsUserExists(request.TenantId, request.UserId))
                {
                    return Result<DeveloperProductivityReportDto>.Failure(
                        $"User with id {request.UserId} does not exist.",
                        HttpStatusCode.NotFound);
                }
            }

            var report = await reportsReadModel.GenerateDeveloperProductivityReportAsync(
                request.TenantId,
                request.From,
                request.To,
                request.ProjectId,
                request.TeamId,
                request.UserId);

            return Result<DeveloperProductivityReportDto>.Success(report, HttpStatusCode.OK);
        }
    }
}