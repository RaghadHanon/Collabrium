namespace Application.Features.AI.DTOs.Roadmaps;
public sealed class RoadmapFullDraftDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public List<string> SuggestedTags { get; set; } = new();

    public List<RoadmapTopicDraftDto> Topics { get; set; } = new();
}