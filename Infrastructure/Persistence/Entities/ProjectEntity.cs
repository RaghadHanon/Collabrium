namespace Infrastructure.Persistence.Entities;

public class ProjectEntity : DbEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public string CreatedByUserId { get; set; } = null!;
    public ApplicationUserEntity CreatedByUser { get; set; } = null!;

    public string TeamId { get; set; } = null!;
    public TeamEntity Team { get; set; } = null!;

    public string TenantId { get; set; } = null!;
    public TenantEntity Tenant { get; set; } = null!;

    public ICollection<SprintEntity> Sprints { get; set; } = new List<SprintEntity>();
    public ICollection<WorkItemEntity> WorkItems { get; set; } = new List<WorkItemEntity>();
}