using Domain.Enums;

namespace Infrastructure.Persistence.Entities;

public class WorkItemRelationEntity : DbEntity
{
    public string SourceWorkItemId { get; set; } = null!;
    public WorkItemEntity SourceWorkItem { get; set; } = null!;

    public string TargetWorkItemId { get; set; } = null!;
    public WorkItemEntity TargetWorkItem { get; set; } = null!;

    public WorkItemRelationType RelationType { get; set; }
}