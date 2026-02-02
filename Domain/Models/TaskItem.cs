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