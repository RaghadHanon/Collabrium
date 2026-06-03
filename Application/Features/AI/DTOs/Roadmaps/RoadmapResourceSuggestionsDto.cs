namespace Application.Features.AI.DTOs.Roadmaps;
public sealed class RoadmapResourceSuggestionsDto
{
    public List<RoadmapResourceSuggestionDto> Suggestions { get; set; } = new();
}