using Domain.Enums;

namespace Infrastructure.Persistence.Entities;

public class WorkItemRelationEntity : DbEntity
{
    public string OutgoingWorkItemId { get; set; } = null!;
    public string IncomingWorkItemId { get; set; } = null!;
    public WorkItemRelationType RelationType { get; set; }

    public WorkItemEntity OutgoingWorkItem { get; set; } = null!;
    public WorkItemEntity IncomingWorkItem { get; set; } = null!;
}
