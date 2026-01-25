using Domain.Enums;

namespace Domain.Models;

public sealed class WorkItemRelation : Entity
{
    public string OutgoingWorkItemId { get; private set; } = null!;
    public string IncomingWorkItemId { get; private set; } = null!;
    public WorkItemRelationType RelationType { get; private set; }

    public WorkItemRelation() { }

    public WorkItemRelation(string id, string outgoingId, string incomingId, WorkItemRelationType type) : base(id)
    {
        OutgoingWorkItemId = outgoingId;
        IncomingWorkItemId = incomingId;
        RelationType = type;
    }
}
