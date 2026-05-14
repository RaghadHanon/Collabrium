using Domain.Enums;

namespace Infrastructure.Persistence.Entities;

public class ProjectEntity : DbEntity
{
    public string Name { get; set; } = null!;
    public string Key { get; set; } = null!;
    public string? Description { get; set; }

    public ProjectStatus Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public int LastWorkItemNumber { get; set; }
    public int LastSprintNumber { get; set; }


    public string CreatedByUserId { get; set; } = null!;
    public ApplicationUserEntity CreatedByUser { get; set; } = null!;

    public string? TeamId { get; set; }
    public TeamEntity? Team { get; set; }

    public string TenantId { get; set; } = null!;
    public TenantEntity Tenant { get; set; } = null!;

    public ICollection<SprintEntity> Sprints { get; set; } = new List<SprintEntity>();
    public ICollection<WorkItemEntity> WorkItems { get; set; } = new List<WorkItemEntity>();
    public ICollection<ProjectHistoryEntity> ProjectHistories { get; set; } = new List<ProjectHistoryEntity>();
}