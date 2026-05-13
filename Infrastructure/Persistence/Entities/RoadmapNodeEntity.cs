namespace Infrastructure.Persistence.Entities;

public abstract class RoadmapNodeEntity : DbEntity
{
    public string RoadmapId { get; set; } = null!;
    public RoadmapEntity Roadmap { get; set; } = null!;
    public string TenantId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }

    public int Order { get; set; }

}
