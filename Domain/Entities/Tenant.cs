namespace Domain.Entities;

public class Tenant : Entity
{
    public string Name { get; set; } = null!;
    public string Domain { get; set; } = null!;
    public bool IsActive { get; set; }

    // Navigation
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

    public ICollection<Team> Teams { get; set; } = new List<Team>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}