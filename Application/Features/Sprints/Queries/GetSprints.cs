using Application.Common;
using Application.Features.Sprints.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.Sprints.Queries;

public class GetSprints
{
    public class Query
        : IRequest<Result<PagedList<SprintDto, DateTime?>>>
    {
        public required string ProjectId { get; set; }

        public string? Code { get; set; }
        public string? NameContains { get; set; }

        public SprintStatus? Status { get; set; }

        public DateTime? StartAfter { get; set; }
        public DateTime? StartBefore { get; set; }

        public DateTime? EndAfter { get; set; }
        public DateTime? EndBefore { get; set; }

        public DateTime? StartedAfter { get; set; }
        public DateTime? StartedBefore { get; set; }

        public DateTime? CompletedAfter { get; set; }
        public DateTime? CompletedBefore { get; set; }

        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }

        public DateTime? Cursor { get; set; }
        public int PageSize { get; set; }
    }

    public class Handler(
        ISprintsReadModel sprintsReadModel,
        ITenantContext tenantContext,
        IMapper mapper,
        IProjectsRepository projectsRepository)
        : IRequestHandler<Query, Result<PagedList<SprintDto, DateTime?>>>
    {
        public async Task<Result<PagedList<SprintDto, DateTime?>>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return Result<PagedList<SprintDto, DateTime?>>.Failure(
                    "Tenant not found.",
                    HttpStatusCode.NotFound);
            }

            var projectExists = await projectsRepository.IsProjectExists(
                tenantId,
                request.ProjectId,
                cancellationToken);

            if (!projectExists)
            {
                return Result<PagedList<SprintDto, DateTime?>>.Failure(
                    "Project not found.",
                    HttpStatusCode.NotFound);
            }

            var parameters =
                mapper.Map<SprintsSearchAndFilterParametersDto>(request);

            parameters.TenantId = tenantId;
            parameters.ProjectId = request.ProjectId;

            var result = await sprintsReadModel.GetSprintsAsync(
                parameters,
                cancellationToken);

            return Result<PagedList<SprintDto, DateTime?>>.Success(
                result,
                HttpStatusCode.OK);
        }
    }
}