using Domain.Enums;

namespace Domain.Core;

public static class WorkItemStatusRules
{
    public static List<WorkItemStatus>? GetAllowedStatusesByType(WorkItemType type)
    {
        return type switch
        {
            WorkItemType.UserStory => Enum.GetValues<WorkItemStatus>().ToList(),
            WorkItemType.TestCase => new List<WorkItemStatus>
                                            {
                                                WorkItemStatus.ToDo,
                                                WorkItemStatus.InTesting,
                                                WorkItemStatus.Done
                                            },
            WorkItemType.TaskItem => new List<WorkItemStatus>
                                            {
                                                WorkItemStatus.ToDo,
                                                WorkItemStatus.ReadyForDev,
                                                WorkItemStatus.InProgress,
                                                WorkItemStatus.InReview,
                                                WorkItemStatus.Done
                                            },
            WorkItemType.Epic => new List<WorkItemStatus>
                                            {
                                                WorkItemStatus.ToDo,
                                                WorkItemStatus.InProgress,
                                                WorkItemStatus.Done
                                            },
            WorkItemType.Bug => new List<WorkItemStatus>
                                            {
                                                WorkItemStatus.ToDo,
                                                WorkItemStatus.ReadyForDev,
                                                WorkItemStatus.InProgress,
                                                WorkItemStatus.InTesting,
                                                WorkItemStatus.Done
                                            },

            _ => null
        };
    }
}