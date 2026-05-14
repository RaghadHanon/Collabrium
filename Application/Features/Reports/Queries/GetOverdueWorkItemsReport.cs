using Application.Common;
using Application.Features.Reports.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Reports.Queries;

public class GetOverdueWorkItemsReport
{
    public class Query : IRequest<Result<OverdueWorkItemsReportDto>>
    {
        public string? TenantId { get; set; }
        public string? ProjectId { get; set; }
        public string? TeamId { get; set; }
        public string? AssignedToUserId { get; set; }
        public DateTime From { get; set; } = DateTime.UtcNow.AddDays(-30);
        public DateTime To { get; set; } = DateTime.UtcNow;
        public int MaxItems { get; set; } = 100;
    }

    public class Handler(
        IReportsReadModel reportsReadModel,
        IProjectsRepository projectsRepository,
        ITeamsRepository teamsRepository,
        IUsersRepository usersRepository,
        ITenantContext tenantContext
    ) : IRequestHandler<Query, Result<OverdueWorkItemsReportDto>>
    {
        public async Task<Result<OverdueWorkItemsReportDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (request.TenantId is null)
            {
                request.TenantId = tenantContext.TenantId;

                if (request.TenantId is null)
                {
                    return Result<OverdueWorkItemsReportDto>.Failure("TenantId is not found.", HttpStatusCode.BadRequest);
                }
            }

            if (request.From > request.To)
            {
                return Result<OverdueWorkItemsReportDto>.Failure("From date cannot be after To date.", HttpStatusCode.BadRequest);
            }

            if (request.MaxItems <= 0)
            {
                return Result<OverdueWorkItemsReportDto>.Failure("MaxItems must be greater than zero.", HttpStatusCode.BadRequest);
            }

            if (request.ProjectId is not null)
            {
                if (!await projectsRepository.IsProjectExists(request.TenantId, request.ProjectId))
                {
                    return Result<OverdueWorkItemsReportDto>.Failure(
                        $"Project with id {request.ProjectId} does not exist.",
                        HttpStatusCode.NotFound);
                }
            }

            if (request.TeamId is not null)
            {
                if (!await teamsRepository.IsTeamExists(request.TenantId, request.TeamId))
                {
                    return Result<OverdueWorkItemsReportDto>.Failure(
                        $"Team with id {request.TeamId} does not exist.",
                        HttpStatusCode.NotFound);
                }
            }

            if (request.AssignedToUserId is not null)
            {
                if (!await usersRepository.IsUserExists(request.TenantId, request.AssignedToUserId))
                {
                    return Result<OverdueWorkItemsReportDto>.Failure(
                        $"User with id {request.AssignedToUserId} does not exist.",
                        HttpStatusCode.NotFound);
                }
            }

            var report = await reportsReadModel.GenerateOverdueWorkItemsReportAsync(
                request.TenantId,
                request.From,
                request.To,
                request.ProjectId,
                request.TeamId,
                request.AssignedToUserId,
                request.MaxItems);

            return Result<OverdueWorkItemsReportDto>.Success(report, HttpStatusCode.OK);
        }
    }
}