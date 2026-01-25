using Domain.Enums;

namespace Infrastructure.Persistence.Entities;

public class TeamEntity : DbEntity
{
    public string TenantId { get; set; } = null!;
    public string TeamLeadId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Department Department { get; set; }
    public int? Capacity { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime UpdatedAt { get; set; }

    public ApplicationUserEntity TeamLead { get; set; } = null!;
    public TenantEntity Tenant { get; set; } = null!;

    public ICollection<ApplicationUserEntity> Members { get; set; } = new List<ApplicationUserEntity>();
    public ICollection<ProjectEntity> Projects { get; set; } = new List<ProjectEntity>();
}
