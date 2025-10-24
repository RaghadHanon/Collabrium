namespace Domain.Entities;

public class Team : Entity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public required string TenantId { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;

    public ICollection<ApplicationUser> Members { get; set; } = new List<ApplicationUser>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}