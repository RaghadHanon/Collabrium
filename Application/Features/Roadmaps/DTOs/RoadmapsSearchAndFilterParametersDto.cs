using Application.Common.DTOs;
using Domain.Enums;

namespace Application.Features.Roadmaps.DTOs;

public sealed class RoadmapsSearchAndFilterParametersDto : QueryParametersDto<DateTime?>
{
    public string? TenantId { get; set; }

    // Text filters
    public string? TitleContains { get; set; }
    public string? DescriptionContains { get; set; }

    // Enums
    public RoadmapPath? Path { get; set; }
    public RoadmapLevel? Level { get; set; }

    // Status flags
    public bool? IsVisible { get; set; }
    public bool? IsPublished { get; set; } // true => PublishedAt != null, false => PublishedAt == null

    // Date ranges
    public DateTime? PublishedAfter { get; set; }
    public DateTime? PublishedBefore { get; set; }

    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }

    public DateTime? UpdatedAfter { get; set; }
    public DateTime? UpdatedBefore { get; set; }
    public string? CreatedByUserId { get; set; }
    public IReadOnlyCollection<string>? TagIdsAny { get; set; }

    public IReadOnlyCollection<string>? TagIdsAll { get; set; }
}
