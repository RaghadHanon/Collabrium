using Domain.Enums;

namespace Domain.Models;

public sealed class Epic : WorkItem
{
    public Epic(string id, string title, string createdByUserId, string projectId)
        : base(id, title, createdByUserId, projectId)
    {
        Type = WorkItemType.Epic;
    }
}
