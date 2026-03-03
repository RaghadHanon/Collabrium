using Application.Common;
using Application.Features.RoadmapResources.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.RoadmapResources.Queries
{
    public sealed class GetResourceById
    {
        public sealed class Query : IRequest<Result<ResourceDto>>
        {
            public required string RoadmapId { get; set; }
            public required string TopicNodeId { get; set; }
            public required string SubTopicId { get; set; }
            public required string ResourceId { get; set; }
        }

        public sealed class Handler(
            IRoadmapResourcesReadModel resourcesReadModel,
            IRoadmapSubTopicsReadModel subTopicsReadModel,
            IRoadmapsRepository roadmapsRepository,
            ITenantContext tenantContext
        ) : IRequestHandler<Query, Result<ResourceDto>>
        {
            public async Task<Result<ResourceDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<ResourceDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var roadmap = await roadmapsRepository.GetByIdAsync(tenantId, request.RoadmapId, cancellationToken);
                if (roadmap is null)
                    return Result<ResourceDto>.Failure("Roadmap not found.", HttpStatusCode.NotFound);

                var subTopic = await subTopicsReadModel.GetByIdAsync(
                    tenantId: tenantId,
                    roadmapId: request.RoadmapId,
                    topicNodeId: request.TopicNodeId,
                    subTopicId: request.SubTopicId,
                    ct: cancellationToken);

                if (subTopic is null)
                    return Result<ResourceDto>.Failure("Subtopic not found.", HttpStatusCode.NotFound);

                var dto = await resourcesReadModel.GetByIdAsync(
                    tenantId: tenantId,
                    roadmapId: request.RoadmapId,
                    topicNodeId: request.TopicNodeId,
                    subTopicId: request.SubTopicId,
                    resourceId: request.ResourceId,
                    ct: cancellationToken);

                return dto is null
                    ? Result<ResourceDto>.Failure("Resource not found.", HttpStatusCode.NotFound)
                    : Result<ResourceDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}
