using Domain.Enums;

namespace Domain.Models;

public abstract class WorkItem : Entity
{
    public string Title { get; private set; } = null!;
    public string? AcceptanceCriteria { get; private set; }
    public string? Description { get; private set; }

    public int EffortPoints { get; private set; }
    public WorkItemType Type { get; protected set; }
    public WorkItemStatus Status { get; private set; }
    public WorkItemPriority Priority { get; private set; }

    public bool IsBlocked { get; private set; }
    public DateTime? UpdatedDate { get; private set; }
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }

    public string? AssignedToUserId { get; private set; }
    public string? AssignedToUserName { get; private set; }

    public string CreatedByUserId { get; private set; } = null!;
    public string ProjectId { get; private set; } = null!;
    public string? SprintId { get; private set; }

    private readonly List<WorkItemRelation> _outgoingRelations = new();
    private readonly List<WorkItemRelation> _incomingRelations = new();

    public IReadOnlyCollection<WorkItemRelation> OutgoingRelations => _outgoingRelations;
    public IReadOnlyCollection<WorkItemRelation> IncomingRelations => _incomingRelations;

    public WorkItem() { }

    public WorkItem(string id, string title, string createdByUserId, string projectId) : base(id)
    {
        Title = title;
        CreatedByUserId = createdByUserId;
        ProjectId = projectId;

        Status = WorkItemStatus.Backlog;
        Priority = WorkItemPriority.Medium;
        UpdatedDate = DateTime.UtcNow;
    }

    public void UpdateDetails(string title, string? description, string? acceptanceCriteria, int effortPoints)
    {
        Title = title;
        Description = description;
        AcceptanceCriteria = acceptanceCriteria;
        EffortPoints = effortPoints;
        Touch();
    }

    public void ChangeStatus(WorkItemStatus status)
    {
        Status = status;
        Touch();
    }

    public void SetPriority(WorkItemPriority priority)
    {
        Priority = priority;
        Touch();
    }

    public void Block() { IsBlocked = true; Touch(); }
    public void Unblock() { IsBlocked = false; Touch(); }

    public void AssignTo(string userId, string? userName = null)
    {
        AssignedToUserId = userId;
        AssignedToUserName = userName;
        Touch();
    }

    public void MoveToSprint(string? sprintId)
    {
        SprintId = sprintId;
        Touch();
    }

    public IReadOnlyCollection<string> GetChildrenIds()
    {
        return _outgoingRelations
            .Where(r => r.RelationType == WorkItemRelationType.Parent)
            .Select(r => r.IncomingWorkItemId)
            .ToList();
    }

    public string? GetParentId()
    {
        return _incomingRelations
            .Where(r => r.RelationType == WorkItemRelationType.Parent)
            .Select(r => r.OutgoingWorkItemId)
            .FirstOrDefault();
    }

    public void AddOutgoingRelation(WorkItemRelation relation)
    {
        _outgoingRelations.Add(relation);
    }

    public void AddIncomingRelation(WorkItemRelation relation)
    {
        _incomingRelations.Add(relation);
    }

    private void Touch()
    {
        UpdatedDate = DateTime.UtcNow;
    }
}
