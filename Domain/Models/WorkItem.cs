using Domain.Common;
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

    private readonly List<CodeLink> _codeLinks = new();
    public IReadOnlyCollection<CodeLink> CodeLinks => _codeLinks.AsReadOnly();

    private readonly List<Attachment> _attachments = new();
    public IReadOnlyCollection<Attachment> Attachments => _attachments.AsReadOnly();

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

    protected WorkItem(string title, string createdByUserId, string projectId)
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

    public void RehydrateAddCodeLink(CodeLink link)
    {
        _codeLinks.Add(link);
    }

    public void RehydrateAddAttachment(Attachment attachment)
    {
        _attachments.Add(attachment);
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

    protected virtual bool CanReopenFromDone => false;

    protected virtual bool IsValidTransition(WorkItemStatus from, WorkItemStatus to)
    {
        return from == to
            || from switch
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

    public void ChangeStatus(WorkItemStatus newStatus)
    {
        Guard.Against(IsBlocked && newStatus == WorkItemStatus.InProgress,
            "Blocked work item cannot be moved to InProgress.");

        if (Status == WorkItemStatus.Done && newStatus != WorkItemStatus.Done)
        {
            Guard.Against(!CanReopenFromDone, "Done work item cannot be reopened.");
        }

        Guard.Against(!IsValidTransition(Status, newStatus),
            $"Invalid status transition: {Status} -> {newStatus}");

        Status = newStatus;

        if (newStatus == WorkItemStatus.InProgress && StartDate is null)
            StartDate = DateTime.UtcNow;

        if (newStatus == WorkItemStatus.Done)
            EndDate = DateTime.UtcNow;

        if (newStatus != WorkItemStatus.Done && EndDate is not null && CanReopenFromDone)
            EndDate = null;

        Touch();
    }

    public void MoveToBacklog()
    {
        ChangeStatus(WorkItemStatus.Backlog);
    }

    public void MoveToReadyForDev()
    {
        ChangeStatus(WorkItemStatus.ReadyForDev);
    }

    public void Start()
    {
        ChangeStatus(WorkItemStatus.InProgress);
    }

    public void SendToReview()
    {
        ChangeStatus(WorkItemStatus.InReview);
    }

    public void SendToTesting()
    {
        ChangeStatus(WorkItemStatus.InTesting);
    }

    public void MarkReadyForRelease()
    {
        ChangeStatus(WorkItemStatus.ReadyForRelease);
    }

    public void MarkDone()
    {
        ChangeStatus(WorkItemStatus.Done);
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
        Guard.Against(IsDone, "Cannot assign a completed work item.");
        AssignedToUserId = userId is null ? null : Guard.NotNullOrWhiteSpace(userId, nameof(userId));
        Touch();
    }

    public void MoveToSprint(string? sprintId)
    {
        Guard.Against(IsDone, "Cannot move a completed work item.");
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

    public void AddRelation(WorkItem target, WorkItemRelationType type)
    {
        Guard.Against(target is null, "Target is required.");
        ValidateRelationRules(this.Type, target!.Type, type);
        AddRelation(target.Id, type);
    }

    private static void ValidateRelationRules(WorkItemType source, WorkItemType target, WorkItemRelationType relationType)
    {
        if (relationType is WorkItemRelationType.Parent)
        {
            var ok = (source, target) switch
            {
                (WorkItemType.UserStory, WorkItemType.Epic) => true,
                (WorkItemType.TaskItem, WorkItemType.UserStory) => true,
                (WorkItemType.Bug, WorkItemType.UserStory) => true,
                (WorkItemType.TestCase, WorkItemType.UserStory) => true,
                _ => false
            };
            Guard.Against(!ok, $"Invalid parent relation: {target} cannot be parent of {source}.");
        }

        if (relationType is WorkItemRelationType.Child)
        {
            var ok = (source, target) switch
            {
                (WorkItemType.Epic, WorkItemType.UserStory) => true,
                (WorkItemType.UserStory, WorkItemType.TaskItem) => true,
                (WorkItemType.UserStory, WorkItemType.Bug) => true,
                (WorkItemType.UserStory, WorkItemType.TestCase) => true,
                _ => false
            };
            Guard.Against(!ok, $"Invalid child relation: {source} cannot have child {target}.");
        }

        if (relationType is WorkItemRelationType.FoundByTestCase)
        {
            var ok = (source, target) is (WorkItemType.Bug, WorkItemType.TestCase);
            Guard.Against(!ok, "FoundByTestCase relation must be Bug -> TestCase.");
        }
    }

    public string? GetParentId()
    {
        return _relations.FirstOrDefault(r => r.RelationType == WorkItemRelationType.Parent)?.TargetWorkItemId;
    }

    public IReadOnlyCollection<string> GetChildrenIds()
    {
        return _relations.Where(r => r.RelationType == WorkItemRelationType.Child)
                .Select(r => r.TargetWorkItemId)
                .Distinct()
                .ToList();
    }

    public IReadOnlyCollection<string> GetRelatedIds(WorkItemRelationType type)
    {
        return _relations.Where(r => r.RelationType == type)
                .Select(r => r.TargetWorkItemId)
                .Distinct()
                .ToList();
    }

    public CodeLink AddCodeLink(string linkId, string url, CodeLinkType type, string? displayName = null)
    {
        url = Guard.NotNullOrWhiteSpace(url, nameof(url)).Trim();

        Guard.Against(_codeLinks.Any(l => l.Url.Equals(url, StringComparison.OrdinalIgnoreCase)),
            "This code link already exists.");

        var link = new CodeLink(linkId, url, type, displayName);
        _codeLinks.Add(link);
        Touch();
        return link;
    }

    public void RemoveCodeLink(string linkId)
    {
        linkId = Guard.NotNullOrWhiteSpace(linkId, nameof(linkId));
        var link = _codeLinks.FirstOrDefault(x => x.Id == linkId);
        Guard.Against(link is null, "Code link not found.");

        _codeLinks.Remove(link);
        Touch();
    }

    public Attachment AddAttachment(
        string attachmentId,
        string fileName,
        string storagePath,
        string contentType,
        long sizeBytes,
        AttachmentType type,
        string uploadedByUserId,
        string? checksum = null)
    {
        Guard.Against(IsDone, "Cannot add attachments to a completed work item."); // optional rule

        var att = new Attachment(
            attachmentId, fileName, storagePath, contentType, type, uploadedByUserId);

        _attachments.Add(att);
        Touch();
        return att;
    }

    public void RemoveAttachment(string attachmentId)
    {
        attachmentId = Guard.NotNullOrWhiteSpace(attachmentId, nameof(attachmentId));
        var att = _attachments.FirstOrDefault(x => x.Id == attachmentId);
        Guard.Against(att is null, "Attachment not found.");

        _attachments.Remove(att);
        Touch();
    }
}