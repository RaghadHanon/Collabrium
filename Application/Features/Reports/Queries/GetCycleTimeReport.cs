using Application.Common;
using Application.Features.Reports.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Reports.Queries;

public class GetCycleTimeReport
{
    public class Query : IRequest<Result<CycleTimeReport>>
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
        ITenantContext tenantContext
        ) : IRequestHandler<Query, Result<CycleTimeReport>>
    {
        public async Task<Result<CycleTimeReport>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (request.TenantId is null)
            {
                request.TenantId = tenantContext.TenantId;
                if (request.TenantId is null)
                {
                    return Result<CycleTimeReport>.Failure("TenantId is not found.", HttpStatusCode.BadRequest);
                }
            }

            if (!await projectsRepository.IsProjectExists(request.TenantId, request.ProjectId))
            {
                return Result<CycleTimeReport>.Failure($"Project with id {request.ProjectId} does not exist.", HttpStatusCode.NotFound);
            }

            if(request.SprintId != null)
            {
                if (!await sprintsRepository.IsExistsAsync(request.TenantId, request.SprintId, request.ProjectId))
                {
                    return Result<CycleTimeReport>.Failure($"Sprint with id {request.SprintId} does not exist in project {request.ProjectId}.", HttpStatusCode.NotFound);
                }
            }

            var report = await reportsReadModel.GenerateCycleTimeReportAsync(request.TenantId, request.ProjectId, request.From, request.To, request.SprintId);

            return Result<CycleTimeReport>.Success(report, HttpStatusCode.OK);
        }
    }
}
