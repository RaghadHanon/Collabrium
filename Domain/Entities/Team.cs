using Domain.Enums;

namespace Domain.Entities;

public class Team : Entity
{
    public required string TenantId { get; set; }
    public required string TeamLeadId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Department Department { get; set; }
    public int? Capacity { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public ApplicationUser TeamLead { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
    public ICollection<ApplicationUser> Members { get; set; } = new List<ApplicationUser>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}