using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

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
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }

    public string? AssignedToUserId { get; private set; }
    public string CreatedByUserId { get; private set; } = null!;
    public string ProjectId { get; private set; } = null!;
    public string? SprintId { get; private set; }

    private readonly List<WorkItemRelation> _relations = new();
    public IReadOnlyCollection<WorkItemRelation> Relations => _relations.AsReadOnly();

    private readonly List<Comment> _comments = new();
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();

    protected WorkItem() { } // EF

    protected WorkItem(string id, string title, string createdByUserId, string projectId) : base(id)
    {
        Title = Guard.NotNullOrWhiteSpace(title, nameof(title));
        CreatedByUserId = Guard.NotNullOrWhiteSpace(createdByUserId, nameof(createdByUserId));
        ProjectId = Guard.NotNullOrWhiteSpace(projectId, nameof(projectId));

        Status = WorkItemStatus.Backlog;
        Priority = WorkItemPriority.Medium;
        Touch();
    }

    protected void RehydrateCommon(
        DateTime createdAt,
        DateTime? updatedAt,
        WorkItemStatus status,
        WorkItemPriority priority,
        bool isBlocked,
        int effortPoints,
        string? description,
        string? acceptanceCriteria,
        string? assignedToUserId,
        string? sprintId,
        DateTime? startDate,
        DateTime? endDate)
    {
        SetTimestamps(createdAt, updatedAt);

        Status = status;
        Priority = priority;
        IsBlocked = isBlocked;

        EffortPoints = effortPoints;
        Description = description;
        AcceptanceCriteria = acceptanceCriteria;

        AssignedToUserId = assignedToUserId;
        SprintId = sprintId;

        StartDate = startDate;
        EndDate = endDate;
    }
    public void RehydrateAddComment(Comment comment)
    {
        _comments.Add(comment);
    }

    public void RehydrateAddRelation(WorkItemRelation relation)
    {
        _relations.Add(relation);
    }

    public bool IsDone => Status == WorkItemStatus.Done;

    public void UpdateDetails(string title, string? description, string? acceptanceCriteria, int effortPoints)
    {
        Title = Guard.NotNullOrWhiteSpace(title, nameof(title));
        EffortPoints = Guard.NonNegative(effortPoints, nameof(effortPoints));

        Description = description?.Trim();
        AcceptanceCriteria = acceptanceCriteria?.Trim();
        Touch();
    }

    public void ChangeStatus(WorkItemStatus newStatus)
    {
        Guard.Against(IsBlocked && newStatus == WorkItemStatus.InProgress,
            "Blocked work item cannot be moved to InProgress.");

        Guard.Against(Status == WorkItemStatus.Done && newStatus != WorkItemStatus.Done,
            "Done work item cannot change status.");

        Guard.Against(!IsValidTransition(Status, newStatus),
            $"Invalid status transition: {Status} -> {newStatus}");

        Status = newStatus;

        if (newStatus == WorkItemStatus.InProgress && StartDate is null)
            StartDate = DateTime.UtcNow;

        if (newStatus == WorkItemStatus.Done)
            EndDate = DateTime.UtcNow;

        Touch();
    }

    private static bool IsValidTransition(WorkItemStatus from, WorkItemStatus to)
    {
        return from == to
            ? true
            : from switch
            {
                WorkItemStatus.Backlog => to is WorkItemStatus.ReadyForDev,
                WorkItemStatus.ReadyForDev => to is WorkItemStatus.InProgress or WorkItemStatus.Backlog,
                WorkItemStatus.InProgress => to is WorkItemStatus.InReview or WorkItemStatus.InTesting,
                WorkItemStatus.InReview => to is WorkItemStatus.InTesting or WorkItemStatus.InProgress,
                WorkItemStatus.InTesting => to is WorkItemStatus.ReadyForRelease or WorkItemStatus.InProgress,
                WorkItemStatus.ReadyForRelease => to is WorkItemStatus.Done or WorkItemStatus.InTesting,
                WorkItemStatus.Done => to is WorkItemStatus.Done,
                _ => false
            };
    }

    public void SetPriority(WorkItemPriority priority)
    {
        Priority = priority;
        Touch();
    }

    public void Block()
    {
        Guard.Against(IsDone, "Cannot block a completed work item.");
        IsBlocked = true;
        Touch();
    }

    public void Unblock()
    {
        IsBlocked = false;
        Touch();
    }

    public void AssignTo(string? userId)
    {
        AssignedToUserId = userId is null ? null : Guard.NotNullOrWhiteSpace(userId, nameof(userId));
        Touch();
    }

    public void MoveToSprint(string? sprintId)
    {
        SprintId = sprintId is null ? null : Guard.NotNullOrWhiteSpace(sprintId, nameof(sprintId));
        Touch();
    }

    public Comment AddComment(string commentId, string createdByUserId, string content)
    {
        Guard.Against(string.IsNullOrWhiteSpace(commentId), "Comment id is required.");
        var comment = new Comment(commentId, createdByUserId, content);
        _comments.Add(comment);
        Touch();
        return comment;
    }

    public void AddRelation(string targetWorkItemId, WorkItemRelationType type)
    {
        targetWorkItemId = Guard.NotNullOrWhiteSpace(targetWorkItemId, nameof(targetWorkItemId));
        Guard.Against(targetWorkItemId == Id, "WorkItem cannot relate to itself.");

        Guard.Against(_relations.Any(r => r.TargetWorkItemId == targetWorkItemId && r.RelationType == type),
            "Relation already exists.");

        if (type == WorkItemRelationType.Parent)
        {
            Guard.Against(_relations.Any(r => r.RelationType == WorkItemRelationType.Parent),
                "WorkItem can have only one parent.");
        }

        _relations.Add(new WorkItemRelation(Guid.NewGuid().ToString(), Id, targetWorkItemId, type));
        Touch();
    }

    public string? GetParentId()
    {
        return _relations.FirstOrDefault(r => r.RelationType == WorkItemRelationType.Parent)?.TargetWorkItemId;
    }

    public IReadOnlyCollection<string> GetChildrenIds()
    {
        return _relations
                .Where(r => r.RelationType == WorkItemRelationType.Child)
                .Select(r => r.TargetWorkItemId)
                .Distinct()
                .ToList();
    }
}