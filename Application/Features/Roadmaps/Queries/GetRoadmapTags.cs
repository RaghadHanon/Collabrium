using Application.Common;
using Application.Features.Tags.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Roadmaps.Queries
{
    public sealed class GetRoadmapTags
    {
        public sealed class Query : IRequest<Result<List<TagDto>>>
        {
            public required string RoadmapId { get; set; }
        }

        public sealed class Handler(
            IRoadmapsRepository roadmapsRepository,
            IRoadmapsReadModel roadmapsReadModel,
            ITenantContext tenantContext
        ) : IRequestHandler<Query, Result<List<TagDto>>>
        {
            public async Task<Result<List<TagDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<List<TagDto>>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var roadmap = await roadmapsRepository.GetByIdAsync(
                    tenantId,
                    request.RoadmapId,
                    cancellationToken);

                if (roadmap is null)
                    return Result<List<TagDto>>.Failure("Roadmap not found.", HttpStatusCode.NotFound);

                var tags = await roadmapsReadModel.GetTagsAsync(
                    tenantId,
                    request.RoadmapId,
                    cancellationToken);

                return Result<List<TagDto>>.Success(tags, HttpStatusCode.OK);
            }
        }
    }
}
