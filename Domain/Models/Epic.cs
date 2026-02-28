using Domain.Common;
using Domain.Enums;

namespace Domain.Models;

public sealed class Epic : WorkItem
{
    private Epic() { }

    public Epic(string id, string title, string createdByUserId, string projectId)
        : base(id, title, createdByUserId, projectId)
    {
        Type = WorkItemType.Epic;
    }

    public Epic(string title, string createdByUserId, string projectId)
           : base(title, createdByUserId, projectId)
    {
        Type = WorkItemType.Epic;
    }

    public static Epic Rehydrate(
        string id,
        DateTime createdAt,
        DateTime? updatedAt,
        string title,
        string createdByUserId,
        string projectId,
        WorkItemType type,
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
        var e = new Epic(id, title, createdByUserId, projectId);
        e.RehydrateCommon(createdAt, updatedAt, type, status, priority, isBlocked, effortPoints,title, description, acceptanceCriteria,
            assignedToUserId, createdByUserId, sprintId, projectId, startDate, endDate);
        return e;
    }

    public override WorkItem Clone()
    {
        var clone = Rehydrate(
            Id,
            CreatedAt,
            UpdatedAt,
            Title,
            CreatedByUserId,
            ProjectId,
            Type,
            Status,
            Priority,
            IsBlocked,
            EffortPoints,
            Description,
            AcceptanceCriteria,
            AssignedToUserId,
            SprintId,
            StartDate,
            EndDate
        );

        return clone;
    }

    protected override bool CanReopenFromDone => true;

    protected override bool IsValidTransition(WorkItemStatus from, WorkItemStatus to)
    {
        // Epic workflow: ToDo -> InProgress  -> Done
        // Allow Done -> InProgress (Reopen)
        return from == to
            || from switch
            {
                WorkItemStatus.ToDo => to is WorkItemStatus.InProgress,
                WorkItemStatus.InProgress => to is WorkItemStatus.Done or WorkItemStatus.ToDo,
                WorkItemStatus.Done => to is WorkItemStatus.InProgress,
                _ => false
            };
    }

    public override void ChangeStatusWithValidation(
    WorkItemStatus newStatus,
    IEnumerable<WorkItemSnapshot>? linked)
    {
        ValidateEpicGate(newStatus, linked);
        base.ChangeStatus(newStatus);
    }

    private void ValidateEpicGate(
        WorkItemStatus target,
        IEnumerable<WorkItemSnapshot>? linked)
    {
        if (target != WorkItemStatus.Done)
            return;

        var items = linked?.ToList() ?? new List<WorkItemSnapshot>();

        bool allUserStoriesDone =
            items.Where(x => x.Type == WorkItemType.UserStory)
                 .All(x => x.Status == WorkItemStatus.Done);

        Guard.Against(!allUserStoriesDone,
            "Cannot complete Epic until all linked UserStories are Done.");
    }
}