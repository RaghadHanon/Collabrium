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
    public sealed class GetRoadmapNodeById
    {
        public sealed class Query : IRequest<Result<RoadmapNodeDto>>
        {
            public required string RoadmapId { get; set; }
            public required string NodeId { get; set; }
        }

        public sealed class Handler(
            IRoadmapNodesReadModel roadmapNodesReadModel,
            IRoadmapsRepository roadmapsRepository,
            ITenantContext tenantContext,
            ICurrentUserAccessor currentUserAccessor
        ) : IRequestHandler<Query, Result<RoadmapNodeDto>>
        {
            public async Task<Result<RoadmapNodeDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<RoadmapNodeDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);
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
                    return Result<RoadmapNodeDto>.Failure("Roadmap not found.", HttpStatusCode.NotFound);

                var dto = await roadmapNodesReadModel.GetByIdAsync(
                    tenantId: tenantId,
                    roadmapId: request.RoadmapId,
                    nodeId: request.NodeId,
                    ct: cancellationToken);

                return dto is null
                    ? Result<RoadmapNodeDto>.Failure("Node not found.", HttpStatusCode.NotFound)
                    : Result<RoadmapNodeDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}
