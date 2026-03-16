using Domain.Enums;

namespace Infrastructure.Persistence.Entities;

public class SprintEntity : DbEntity
{
    public string TenantId { get; set; } = null!;

    public string Code { get; set; } = null!;
    public string? Goal { get; set; } 
    public string? Name { get; set; } 
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SprintStatus Status { get; set; }

    public string ProjectId { get; set; } = null!;
    public ProjectEntity Project { get; set; } = null!;

    public ICollection<WorkItemEntity> WorkItems { get; set; } = new List<WorkItemEntity>();
}