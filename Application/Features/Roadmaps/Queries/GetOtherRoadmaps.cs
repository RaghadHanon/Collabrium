using Application.Common;
using Application.Features.Roadmaps.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using MediatR;
using System.Net;

namespace Application.Features.Roadmaps.Queries;

public sealed class GetOtherRoadmaps
{
    public sealed class Query : IRequest<Result<List<RoadmapDto>>>
    {
        public required string CurrentRoadmapId { get; set; }
    }
    public sealed class Handler(
        IRoadmapsReadModel readModel,
        ITenantContext tenantContext
    ) : IRequestHandler<Query, Result<List<RoadmapDto>>>
    {
        public async Task<Result<List<RoadmapDto>>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;

            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<List<RoadmapDto>>.Failure(
                    "Tenant not found", HttpStatusCode.NotFound);

            var roadmaps = await readModel.GetOtherRoadmapsAsync(
                tenantId,
                request.CurrentRoadmapId,
                cancellationToken);

            return Result<List<RoadmapDto>>.Success(roadmaps, HttpStatusCode.OK);
        }
    }
}
