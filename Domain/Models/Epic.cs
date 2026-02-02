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

    public static Epic Rehydrate(
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
        var e = new Epic(id, title, createdByUserId, projectId);
        e.RehydrateCommon(createdAt, updatedAt, status, priority, isBlocked, effortPoints, description, acceptanceCriteria,
            assignedToUserId, sprintId, startDate, endDate);
        return e;
    }
}