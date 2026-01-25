using Domain.Enums;

namespace Domain.Models;

public sealed class TaskItem : WorkItem
{
    public TaskItem(string id, string title, string createdByUserId, string projectId)
        : base(id, title, createdByUserId, projectId)
    {
        Type = WorkItemType.TaskItem;
    }
}
