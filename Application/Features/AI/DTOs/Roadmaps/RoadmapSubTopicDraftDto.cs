namespace Application.Features.AI.DTOs.Roadmaps;
public sealed class RoadmapSubTopicDraftDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public List<RoadmapResourceDraftDto> Resources { get; set; } = new();
}