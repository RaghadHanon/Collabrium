using Application.Features.Roadmaps.DTOs;

namespace Application.Features.RoadmapNodes.DTOs;

public sealed class TopicNodeDetailsDto
{
    public required string Id { get; set; }
    public required string RoadmapId { get; set; }

    public required string Title { get; set; }
    public string? Description { get; set; }

    public int Order { get; set; }

    public int SubTopicsCount { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public DateTime? CreatedAt { get; set; }

    public IReadOnlyCollection<SubTopicDto> SubTopics { get; set; }
        = Array.Empty<SubTopicDto>();
}
