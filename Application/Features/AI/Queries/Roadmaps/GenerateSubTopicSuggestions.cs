using Application.Common;
using Application.Features.AI.DTOs.Roadmaps;
using Application.Interfaces;
using Application.Interfaces.AI;
using Application.Interfaces.ReadModels;
using MediatR;
using System.Net;

namespace Application.Features.AI.Queries.Roadmaps;
public static class GenerateSubTopicSuggestions
{
    public sealed class Query : IRequest<Result<RoadmapSubTopicSuggestionsDto>>
    {
        public string RoadmapId { get; set; } = string.Empty;
        public string TopicNodeId { get; set; } = string.Empty;
        public int SuggestionsCount { get; set; } = 3;
    }

    public sealed class Handler(
        IRoadmapsReadModel roadmapsReadModel,
        IRoadmapNodesReadModel roadmapNodesReadModel,
        IRoadmapSubTopicsReadModel subTopicsReadModel,
        IRoadmapAiSuggestionService aiSuggestionService,
        ITenantContext tenantContext)
        : IRequestHandler<Query, Result<RoadmapSubTopicSuggestionsDto>>
    {
        public async Task<Result<RoadmapSubTopicSuggestionsDto>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<RoadmapSubTopicSuggestionsDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var roadmap = await roadmapsReadModel.GetByIdAsync(
                tenantId,
                request.RoadmapId,
                cancellationToken);

            if (roadmap is null)
                return Result<RoadmapSubTopicSuggestionsDto>.Failure("Roadmap not found.", HttpStatusCode.NotFound);

            var topic = await roadmapNodesReadModel.GetByIdAsync(
                tenantId,
                request.RoadmapId,
                request.TopicNodeId,
                cancellationToken);

            if (topic is null || topic.NodeType != "Topic")
                return Result<RoadmapSubTopicSuggestionsDto>.Failure("Topic not found.", HttpStatusCode.NotFound);

            var subTopics = await subTopicsReadModel.GetByTopicAsync(
                tenantId,
                request.RoadmapId,
                request.TopicNodeId,
                cancellationToken);

            var existingSubTopicTitles = subTopics
                .Select(x => x.Title)
                .ToList();

            var suggestions = await aiSuggestionService.SuggestSubTopicsAsync(
                roadmap.Title,
                topic.Title,
                topic.Description,
                existingSubTopicTitles,
                request.SuggestionsCount,
                cancellationToken);

            return Result<RoadmapSubTopicSuggestionsDto>.Success(suggestions, HttpStatusCode.OK);
        }
    }
}