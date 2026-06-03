using Domain.Enums;

namespace Application.Features.AI.DTOs.Roadmaps;
public sealed class RoadmapResourceDraftDto
{
    public string Title { get; set; } = string.Empty;

    public RoadmapResourceType Type { get; set; }

    public string SearchKeyword { get; set; } = string.Empty;
}