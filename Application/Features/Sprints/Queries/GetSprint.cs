using Application.Common;
using Application.Features.Sprints.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using MediatR;
using System.Net;

namespace Application.Features.Sprints.Queries;

public class GetSprint
{
    public class Query : IRequest<Result<SprintDto>>
    {
        public required string ProjectId { get; set; }
        public required string SprintId { get; set; }
    }

    public class Handler(
        ISprintsReadModel sprintsReadModel,
        ITenantContext tenantContext) : IRequestHandler<Query, Result<SprintDto>>
    {
        public async Task<Result<SprintDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<SprintDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var dto = await sprintsReadModel.GetByIdAsync(tenantId, request.SprintId, request.ProjectId, cancellationToken);

            return dto is null
                ? Result<SprintDto>.Failure("Sprint not found.", HttpStatusCode.NotFound)
                : Result<SprintDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}