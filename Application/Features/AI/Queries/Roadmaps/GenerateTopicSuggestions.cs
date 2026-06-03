using Application.Common;
using Application.Features.AI.DTOs.Roadmaps;
using Application.Interfaces;
using Application.Interfaces.AI;
using Application.Interfaces.ReadModels;
using MediatR;
using System.Net;

namespace Application.Features.AI.Queries.Roadmaps;
public static class GenerateTopicSuggestions
{
    public sealed class Query : IRequest<Result<RoadmapTopicSuggestionsDto>>
    {
        public string RoadmapId { get; set; } = string.Empty;
        public int SuggestionsCount { get; set; } = 3;
    }

    public sealed class Handler(
        IRoadmapsReadModel roadmapsReadModel,
        IRoadmapNodesReadModel roadmapNodesReadModel,
        IRoadmapAiSuggestionService aiSuggestionService,
        ITenantContext tenantContext)
        : IRequestHandler<Query, Result<RoadmapTopicSuggestionsDto>>
    {
        public async Task<Result<RoadmapTopicSuggestionsDto>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<RoadmapTopicSuggestionsDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var roadmap = await roadmapsReadModel.GetByIdAsync(
                tenantId,
                request.RoadmapId,
                cancellationToken);

            if (roadmap is null)
                return Result<RoadmapTopicSuggestionsDto>.Failure("Roadmap not found.", HttpStatusCode.NotFound);

            var nodes = await roadmapNodesReadModel.GetByRoadmapIdAsync(
                tenantId,
                request.RoadmapId,
                cancellationToken);

            var existingTopicTitles = nodes
                .Where(x => x.NodeType == "Topic")
                .Select(x => x.Title)
                .ToList();

            var suggestions = await aiSuggestionService.SuggestTopicsAsync(
                roadmap.Title,
                roadmap.Description,
                roadmap.Path.ToString(),
                roadmap.Level.ToString(),
                existingTopicTitles,
                request.SuggestionsCount,
                cancellationToken);

            return Result<RoadmapTopicSuggestionsDto>.Success(suggestions, HttpStatusCode.OK);
        }
    }
}