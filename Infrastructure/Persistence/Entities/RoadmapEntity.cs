using Domain.Enums;

namespace Infrastructure.Persistence.Entities;

public class RoadmapEntity : DbEntity
{
    public RoadmapPath Path { get; set; }
    public RoadmapLevel Level { get; set; }

    public string Title { get; set; } = null!;
    public string? Description { get; set; }

    public DateTime? PublishedAt { get; set; }
    public bool IsVisible { get; set; } = true;

    public string TenantId { get; set; } = null!;
    public TenantEntity Tenant { get; set; } = null!;

    public string? CreatedByUserId { get; set; }
    public ApplicationUserEntity? CreatedByUser { get; set; }

    // Join table
    public ICollection<TagEntity> Tags { get; set; } = new List<TagEntity>();
    // Content
    public ICollection<RoadmapNodeEntity> Nodes { get; set; } = new List<RoadmapNodeEntity>();
}