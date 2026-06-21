using Domain.Enums;

namespace Application.Features.WorkItems.Rules;

public static class WorkItemSprintHierarchyRules
{
    public static bool IsUserStoryChildType(
        WorkItemType workItemType)
    {
        return workItemType is
            WorkItemType.TaskItem or
            WorkItemType.Bug or
            WorkItemType.TestCase;
    }

    public static bool RequiresSameSprint(
        WorkItemType sourceType,
        WorkItemType targetType,
        WorkItemRelationType relationType)
    {
        if (relationType is not
            (WorkItemRelationType.Parent or
             WorkItemRelationType.Child))
        {
            return false;
        }

        return IsUserStoryHierarchy(
            sourceType,
            targetType);
    }

    public static bool IsUserStoryHierarchy(
        WorkItemType sourceType,
        WorkItemType targetType)
    {
        return
            sourceType == WorkItemType.UserStory
            && IsUserStoryChildType(targetType)
            ||
            targetType == WorkItemType.UserStory
            && IsUserStoryChildType(sourceType);
    }

    public static bool AreInSameSprint(
        string? firstSprintId,
        string? secondSprintId)
    {
        return string.Equals(
            firstSprintId,
            secondSprintId,
            StringComparison.Ordinal);
    }
}