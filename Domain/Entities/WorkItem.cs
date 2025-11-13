using Domain.Enums;

namespace Domain.Entities;

public class WorkItem : Entity
{
    public string Title { get; set; } = null!;
    public string? AcceptanceCriteria { get; set; }
    public string? Description { get; set; }
    public int EffortPoints { get; set; }
    public WorkItemType Type { get; set; }
    public WorkItemStatus Status { get; set; }
    public WorkItemPriority Priority { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public string CreatedByUserId { get; set; } = null!;
    public required string ProjectId { get; set; }
    public string? SprintId { get; set; }

    // Navigation
    public ApplicationUser? AssignedToUser { get; set; }

    public ApplicationUser CreatedByUser { get; set; } = null!;
    public Project Project { get; set; } = null!;
    public Sprint? Sprint { get; set; }
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<WorkItemRelation> OutgoingRelations { get; set; } = new List<WorkItemRelation>();
    public ICollection<WorkItemRelation> IncomingRelations { get; set; } = new List<WorkItemRelation>();

    public ICollection<WorkItem> GetChildren()
    {
        return OutgoingRelations
           .Where(r => r.RelationType == WorkItemRelationType.Parent)
           .Select(r => r.IncomingWorkItem)
           .ToList();
    }

    public WorkItem? GetParent()
    {
        return IncomingRelations
            .Where(r => r.RelationType == WorkItemRelationType.Parent)
            .Select(r => r.OutgoingWorkItem)
            .FirstOrDefault();
    }
}
