using Application.Common;
using Application.Features.SubTopics.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.SubTopics.Queries
{
    public sealed class GetSubTopicDetails
    {
        public sealed class Query : IRequest<Result<SubTopicDetailsDto>>
        {
            public required string RoadmapId { get; set; }
            public required string TopicNodeId { get; set; }
            public required string SubTopicId { get; set; }
        }

        public sealed class Handler(
            IRoadmapSubTopicsReadModel subTopicsReadModel,
            IRoadmapsRepository roadmapsRepository,
            ITenantContext tenantContext
        ) : IRequestHandler<Query, Result<SubTopicDetailsDto>>
        {
            public async Task<Result<SubTopicDetailsDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<SubTopicDetailsDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var roadmap = await roadmapsRepository.GetByIdAsync(tenantId, request.RoadmapId, cancellationToken);
                if (roadmap is null)
                    return Result<SubTopicDetailsDto>.Failure("Roadmap not found.", HttpStatusCode.NotFound);

                var topicExists = await subTopicsReadModel.TopicExistsInRoadmapAsync(
                    tenantId: tenantId,
                    roadmapId: request.RoadmapId,
                    topicNodeId: request.TopicNodeId,
                    ct: cancellationToken);

                if (!topicExists)
                    return Result<SubTopicDetailsDto>.Failure("Topic node not found.", HttpStatusCode.NotFound);

                var dto = await subTopicsReadModel.GetDetailsAsync(
                    tenantId: tenantId,
                    roadmapId: request.RoadmapId,
                    topicNodeId: request.TopicNodeId,
                    subTopicId: request.SubTopicId,
                    ct: cancellationToken);

                return dto is null
                    ? Result<SubTopicDetailsDto>.Failure("SubTopic not found.", HttpStatusCode.NotFound)
                    : Result<SubTopicDetailsDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}
