namespace Domain.Entities;

public class Project : Entity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string CreatedByUserId { get; set; } = null!;
    public required string TeamId { get; set; }
    public required string TenantId { get; set; }

    // Navigation
    public ApplicationUser CreatedByUser { get; set; } = null!;

    public Team Team { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();
    public ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
}