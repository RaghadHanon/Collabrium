using Application.Features.Tags.DTOs;
using Domain.Enums;

namespace Application.Features.Roadmaps.DTOs;

public class RoadmapDto
{
    public required string Id { get; set; }
    public required string TenantId { get; set; }

    public required string Title { get; set; }
    public string? Description { get; set; }

    public RoadmapPath Path { get; set; }
    public RoadmapLevel Level { get; set; }

    public bool IsVisible { get; set; }
    public DateTime? PublishedAt { get; set; }

    public string? CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }

    public int TagsCount { get; set; }
    public int NodesCount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public IReadOnlyCollection<TagDto> Tags { get; set; } = Array.Empty<TagDto>();
}
