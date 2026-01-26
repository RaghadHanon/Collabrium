using Domain.Enums;
using Infrastructure.Persistence.Entities.Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Entities;

public class TeamEntity : DbEntity
{
    public string TenantId { get; set; } = null!;
    public TenantEntity Tenant { get; set; } = null!;

    public string TeamLeadId { get; set; } = null!;
    public ApplicationUserEntity TeamLead { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Department Department { get; set; }
    public int? Capacity { get; set; }
    public bool IsActive { get; set; } = true;

    public new DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ✅ Join Entity بدل Many-to-Many مباشرة
    public ICollection<TeamMemberEntity> Members { get; set; } = new List<TeamMemberEntity>();

    public ICollection<ProjectEntity> Projects { get; set; } = new List<ProjectEntity>();
}