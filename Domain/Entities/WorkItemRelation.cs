using Domain.Enums;

namespace Domain.Entities;

public sealed class WorkItemRelation : Entity
{
    public string SourceWorkItemId { get; private set; } = null!;
    public string TargetWorkItemId { get; private set; } = null!;
    public WorkItemRelationType RelationType { get; private set; }

    private WorkItemRelation() { }

    public WorkItemRelation(string id, string sourceId, string targetId, WorkItemRelationType type) : base(id)
    {
        SourceWorkItemId = sourceId;
        TargetWorkItemId = targetId;
        RelationType = type;
    }

    public static WorkItemRelation Rehydrate(
        string id,
        DateTime createdAt,
        DateTime? updatedAt,
        string sourceId,
        string targetId,
        WorkItemRelationType type)
    {
        var r = new WorkItemRelation(id, sourceId, targetId, type);
        r.SetTimestamps(createdAt, updatedAt);
        return r;
    }
}