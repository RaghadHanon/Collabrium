namespace Application.Features.AI.DTOs.Roadmaps;
public sealed class RoadmapTopicSuggestionsDto
{
    public List<RoadmapTopicSuggestionDto> Suggestions { get; set; } = new();
}