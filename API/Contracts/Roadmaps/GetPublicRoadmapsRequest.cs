using Domain.Enums;

namespace API.Contracts.Roadmaps;

public class GetPublicRoadmapsRequest
{
    public string? TitleContains { get; set; }
    public string? DescriptionContains { get; set; }

    public RoadmapPath? Path { get; set; }
    public RoadmapLevel? Level { get; set; }
    public DateTime? PublishedAfter { get; set; }
    public DateTime? PublishedBefore { get; set; }

    public DateTime? UpdatedAfter { get; set; }
    public DateTime? UpdatedBefore { get; set; }

    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public string? CreatedByUserId { get; set; }

    public IReadOnlyCollection<string>? TagIdsAny { get; set; }
    public IReadOnlyCollection<string>? TagIdsAll { get; set; }

    public DateTime? Cursor { get; set; }
    public int PageSize { get; set; } = 10;
}
