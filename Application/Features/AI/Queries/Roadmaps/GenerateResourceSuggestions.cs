using Application.Common;
using Application.Features.AI.DTOs.Roadmaps;
using Application.Interfaces;
using Application.Interfaces.AI;
using Application.Interfaces.ReadModels;
using MediatR;
using System.Net;

namespace Application.Features.AI.Queries.Roadmaps;
public static class GenerateResourceSuggestions
{
    public sealed class Query : IRequest<Result<RoadmapResourceSuggestionsDto>>
    {
        public string RoadmapId { get; set; } = string.Empty;
        public string TopicNodeId { get; set; } = string.Empty;
        public string SubTopicId { get; set; } = string.Empty;
        public int SuggestionsCount { get; set; } = 3;
    }

    public sealed class Handler(
        IRoadmapsReadModel roadmapsReadModel,
        IRoadmapNodesReadModel roadmapNodesReadModel,
        IRoadmapSubTopicsReadModel subTopicsReadModel,
        IRoadmapResourcesReadModel resourcesReadModel,
        IRoadmapAiSuggestionService aiSuggestionService,
        ITenantContext tenantContext)
        : IRequestHandler<Query, Result<RoadmapResourceSuggestionsDto>>
    {
        public async Task<Result<RoadmapResourceSuggestionsDto>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<RoadmapResourceSuggestionsDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var roadmap = await roadmapsReadModel.GetByIdAsync(
                tenantId,
                request.RoadmapId,
                cancellationToken);

            if (roadmap is null)
                return Result<RoadmapResourceSuggestionsDto>.Failure("Roadmap not found.", HttpStatusCode.NotFound);

            var topic = await roadmapNodesReadModel.GetByIdAsync(
                tenantId,
                request.RoadmapId,
                request.TopicNodeId,
                cancellationToken);

            if (topic is null || topic.NodeType != "Topic")
                return Result<RoadmapResourceSuggestionsDto>.Failure("Topic not found.", HttpStatusCode.NotFound);

            var subTopic = await subTopicsReadModel.GetByIdAsync(
                tenantId,
                request.RoadmapId,
                request.TopicNodeId,
                request.SubTopicId,
                cancellationToken);

            if (subTopic is null)
                return Result<RoadmapResourceSuggestionsDto>.Failure("Subtopic not found.", HttpStatusCode.NotFound);

            var resources = await resourcesReadModel.GetBySubTopicIdAsync(
                 tenantId,
                 request.RoadmapId,
                 request.TopicNodeId,
                 request.SubTopicId,
                 cancellationToken);

            var existingResourceTitles = resources
                .Select(x => x.Title)
                .ToList();

            var suggestions = await aiSuggestionService.SuggestResourcesAsync(
                roadmap.Title,
                topic.Title,
                subTopic.Title,
                subTopic.Description,
                existingResourceTitles,
                request.SuggestionsCount,
                cancellationToken);

            return Result<RoadmapResourceSuggestionsDto>.Success(suggestions, HttpStatusCode.OK);
        }
    }
}