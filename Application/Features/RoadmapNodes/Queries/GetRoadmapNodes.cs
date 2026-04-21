using Application.Common;
using Application.Features.RoadmapNodes.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.RoadmapNodes.Queries
{
    public sealed class GetRoadmapNodes
    {
        public sealed class Query : IRequest<Result<List<RoadmapNodeDto>>>
        {
            public required string RoadmapId { get; set; }
        }

        public sealed class Handler(
            IRoadmapNodesReadModel roadmapNodesReadModel,
            ITenantContext tenantContext,
            IRoadmapsRepository roadmapsRepository,
            ICurrentUserAccessor currentUserAccessor
        ) : IRequestHandler<Query, Result<List<RoadmapNodeDto>>>
        {
            public async Task<Result<List<RoadmapNodeDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<List<RoadmapNodeDto>>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var userId = currentUserAccessor.GetUserId();

                var canViewHidden = await RoadmapAccessHelper.CanCurrentUserViewHiddenAsync(
                  currentUserAccessor,
                  cancellationToken);

                var roadmap = await roadmapsRepository.GetByIdAsync(
               tenantId,
               request.RoadmapId,
               includeHidden: canViewHidden,
               cancellationToken);

                if (roadmap is null)
                    return Result<List<RoadmapNodeDto>>.Failure("Roadmap not found.", HttpStatusCode.NotFound);

                var nodes = await roadmapNodesReadModel.GetByRoadmapIdAsync(
                    tenantId: tenantId,
                    roadmapId: request.RoadmapId,
                    ct: cancellationToken);

                return Result<List<RoadmapNodeDto>>.Success(nodes, HttpStatusCode.OK);
            }
        }
    }
}
