using Domain.Enums;

namespace Domain.Models;

public sealed class TaskItem : WorkItem
{
    private TaskItem() { }

    public TaskItem(string id, string title, string createdByUserId, string projectId)
        : base(id, title, createdByUserId, projectId)
    {
        Type = WorkItemType.TaskItem;
    }

    public TaskItem(string title, string createdByUserId, string projectId)
        : base(title, createdByUserId, projectId)
    {
        Type = WorkItemType.TaskItem;
    }

    protected override bool IsValidTransition(WorkItemStatus from, WorkItemStatus to)
    {
        // Task workflow: ToDo -> ReadyForDev -> InProgress -> InReview -> Done
        return from == to
            || from switch
            {
                WorkItemStatus.ToDo => to is WorkItemStatus.ReadyForDev,
                WorkItemStatus.ReadyForDev => to is WorkItemStatus.InProgress or WorkItemStatus.ToDo,
                WorkItemStatus.InProgress => to is WorkItemStatus.InReview or WorkItemStatus.ReadyForDev,
                WorkItemStatus.InReview => to is WorkItemStatus.Done or WorkItemStatus.InProgress,
                WorkItemStatus.Done => to is WorkItemStatus.InReview or WorkItemStatus.InProgress,
                _ => false
            };
    }

    public static TaskItem Rehydrate(
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
        var t = new TaskItem(id, title, createdByUserId, projectId);
        t.RehydrateCommon(createdAt, updatedAt, type, status, priority, isBlocked, effortPoints, title, description, acceptanceCriteria,
            assignedToUserId, createdByUserId, sprintId, projectId, startDate, endDate);
        return t;
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
}