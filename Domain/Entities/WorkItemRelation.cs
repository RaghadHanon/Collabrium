using Domain.Enums;

namespace Domain.Entities;

public class WorkItemRelation : Entity
{
    public required string OutgoingWorkItemId { get; set; }
    public required string IncomingWorkItemId { get; set; }
    public WorkItemRelationType RelationType { get; set; }

    // Navigation
    public WorkItem OutgoingWorkItem { get; set; } = null!;

    public WorkItem IncomingWorkItem { get; set; } = null!;
}