namespace Domain.Entities;

public class Tenant : Entity
{
    public string Name { get; set; } = null!;
    public string Domain { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public string? TenantAdminId { get; set; }
    public string? profileImageURL { get; set; }

    // Navigation
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

    public ICollection<Team> Teams { get; set; } = new List<Team>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}