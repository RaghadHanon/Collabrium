using Application.Common;
using Application.Features.Reports.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Reports.Queries;

public class GetSprintsProgressReport
{
    public class Query : IRequest<Result<SprintProgressDto>>
    {
        public required string ProjectId { get; set; }
        public required string SprintId { get; set; }
    }

    public class Handler(
        IReportsReadModel reportsReadModel,
        IProjectsRepository projectsRepository,
        ISprintsRepository sprintsRepository,
        ITenantContext tenantContext
        ) : IRequestHandler<Query, Result<SprintProgressDto>>
    {
        public async Task<Result<SprintProgressDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.TenantId;
            if (string.IsNullOrEmpty(tenantId))
                return Result<SprintProgressDto>.Failure("Tenant not found.", HttpStatusCode.BadRequest);

            if (!await projectsRepository.IsProjectExists(tenantId, request.ProjectId, cancellationToken))
                return Result<SprintProgressDto>.Failure("Project is not found.", HttpStatusCode.BadRequest);

            if (!await sprintsRepository.IsExistsAsync(tenantId, request.SprintId, request.ProjectId, cancellationToken))
                return Result<SprintProgressDto>.Failure("Sprint is not found.", HttpStatusCode.BadRequest);

            var report = await reportsReadModel.GenerateSprintProgressReport(tenantId, request.ProjectId, request.SprintId);

            return Result<SprintProgressDto>.Success(report, HttpStatusCode.OK);
        }
    }
}