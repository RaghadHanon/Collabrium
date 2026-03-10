using Application.Common;
using Application.Features.Roadmaps.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.SubTopics.Queries
{
    public sealed class GetSubTopicsByTopic
    {
        public sealed class Query : IRequest<Result<List<SubTopicDto>>>
        {
            public required string RoadmapId { get; set; }
            public required string TopicNodeId { get; set; }
        }

        public sealed class Handler(
            IRoadmapSubTopicsReadModel subTopicsReadModel,
            IRoadmapsRepository roadmapsRepository,
            ITenantContext tenantContext
        ) : IRequestHandler<Query, Result<List<SubTopicDto>>>
        {
            public async Task<Result<List<SubTopicDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<List<SubTopicDto>>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var roadmap = await roadmapsRepository.GetByIdAsync(tenantId, request.RoadmapId, cancellationToken);
                if (roadmap is null)
                    return Result<List<SubTopicDto>>.Failure("Roadmap not found.", HttpStatusCode.NotFound);

                var topicExists = await subTopicsReadModel.TopicExistsInRoadmapAsync(
                    tenantId: tenantId,
                    roadmapId: request.RoadmapId,
                    topicNodeId: request.TopicNodeId,
                    ct: cancellationToken);

                if (!topicExists)
                    return Result<List<SubTopicDto>>.Failure("Topic node not found.", HttpStatusCode.NotFound);

                var items = await subTopicsReadModel.GetByTopicAsync(
                    tenantId: tenantId,
                    roadmapId: request.RoadmapId,
                    topicNodeId: request.TopicNodeId,
                    cancellationToken: cancellationToken);

                return Result<List<SubTopicDto>>.Success(items, HttpStatusCode.OK);
            }
        }
    }
}

