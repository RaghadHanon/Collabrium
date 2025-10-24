using Domain.Enums;

namespace Domain.Entities;

public class Sprint : Entity
{
    public string Name { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SprintStatus Status { get; set; }
    public required string ProjectId { get; set; }

    // Navigation
    public Project Project { get; set; } = null!;

    public ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
}