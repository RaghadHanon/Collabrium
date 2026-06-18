namespace Application.Features.AI.DTOs.Roadmaps;
public sealed class RoadmapTopicDraftDto
{
    public int Order { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public List<RoadmapSubTopicDraftDto> SubTopics { get; set; } = new();
}