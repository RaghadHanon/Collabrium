using Application.Common;
using Application.Features.Roadmaps.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.SubTopics.Queries
{
    public sealed class GetSubTopicById
    {
        public sealed class Query : IRequest<Result<SubTopicDto>>
        {
            public required string RoadmapId { get; set; }
            public required string TopicNodeId { get; set; }
            public required string SubTopicId { get; set; }
        }

        public sealed class Handler(
            IRoadmapSubTopicsReadModel subTopicsReadModel,
            IRoadmapsRepository roadmapsRepository,
            ITenantContext tenantContext
        ) : IRequestHandler<Query, Result<SubTopicDto>>
        {
            public async Task<Result<SubTopicDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<SubTopicDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var roadmap = await roadmapsRepository.GetByIdAsync(tenantId, request.RoadmapId, cancellationToken);
                if (roadmap is null)
                    return Result<SubTopicDto>.Failure("Roadmap not found.", HttpStatusCode.NotFound);

                var topicExists = await subTopicsReadModel.TopicExistsInRoadmapAsync(
                    tenantId: tenantId,
                    roadmapId: request.RoadmapId,
                    topicNodeId: request.TopicNodeId,
                    ct: cancellationToken);

                if (!topicExists)
                    return Result<SubTopicDto>.Failure("Topic node not found.", HttpStatusCode.NotFound);

                var dto = await subTopicsReadModel.GetByIdAsync(
                    tenantId: tenantId,
                    roadmapId: request.RoadmapId,
                    topicNodeId: request.TopicNodeId,
                    subTopicId: request.SubTopicId,
                    ct: cancellationToken);

                return dto is null
                    ? Result<SubTopicDto>.Failure("SubTopic not found.", HttpStatusCode.NotFound)
                    : Result<SubTopicDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}
