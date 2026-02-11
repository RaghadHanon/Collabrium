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
                WorkItemStatus.Done => to is WorkItemStatus.Done,
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
        t.RehydrateCommon(createdAt, updatedAt, status, priority, isBlocked, effortPoints, description, acceptanceCriteria,
            assignedToUserId, sprintId, startDate, endDate);
        return t;
    }
}