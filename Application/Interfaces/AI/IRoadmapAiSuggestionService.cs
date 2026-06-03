using Application.Features.AI.DTOs.Roadmaps;

namespace Application.Interfaces.AI;
public interface IRoadmapAiSuggestionService
{
    Task<RoadmapTopicSuggestionsDto> SuggestTopicsAsync(
        string roadmapTitle,
        string? roadmapDescription,
        string path,
        string level,
        IReadOnlyCollection<string> existingTopicTitles,
        int suggestionsCount,
        CancellationToken cancellationToken = default);

    Task<RoadmapSubTopicSuggestionsDto> SuggestSubTopicsAsync(
        string roadmapTitle,
        string topicTitle,
        string? topicDescription,
        IReadOnlyCollection<string> existingSubTopicTitles,
        int suggestionsCount,
        CancellationToken cancellationToken = default);

    Task<RoadmapResourceSuggestionsDto> SuggestResourcesAsync(
        string roadmapTitle,
        string topicTitle,
        string subTopicTitle,
        string? subTopicDescription,
        IReadOnlyCollection<string> existingResourceTitles,
        int suggestionsCount,
        CancellationToken cancellationToken = default);
}