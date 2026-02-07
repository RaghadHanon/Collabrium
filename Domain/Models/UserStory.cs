using Domain.Common;
using Domain.Enums;

namespace Domain.Models;

public sealed class UserStory : WorkItem
{
    public int StoryPoints { get; private set; }

    private UserStory() { }

    public UserStory(string id, string title, string createdByUserId, string projectId, int storyPoints)
        : base(id, title, createdByUserId, projectId)
    {
        Type = WorkItemType.UserStory;
        Estimate(storyPoints);
    }

    public void Estimate(int points)
    {
        Guard.Against(points < 0, "StoryPoints cannot be negative.");
        Guard.Against(points > 100, "StoryPoints is too high.");
        StoryPoints = points;
        Touch();
    }

    /// <summary>
    /// Returns true if Story can move to InTesting based on linked items snapshots.
    /// Typical rule: all Tasks done AND no open Bugs.
    /// </summary>
    public bool CanMoveToTesting(IEnumerable<WorkItemSnapshot> linked)
    {
        var items = linked?.ToList() ?? new List<WorkItemSnapshot>();

        var allTasksDone = items.Where(x => x.Type == WorkItemType.TaskItem).All(x => x.Status == WorkItemStatus.Done);

        var noOpenBugs = items.Where(x => x.Type == WorkItemType.Bug).All(x => x.Status == WorkItemStatus.Done);

        return allTasksDone && noOpenBugs;
    }

    /// <summary>
    /// Returns true if Story can be Done: Tasks Done + Bugs Done + TestCases Passed.
    /// </summary>
    public bool CanBeCompleted(IEnumerable<WorkItemSnapshot> linked)
    {
        var items = linked?.ToList() ?? new List<WorkItemSnapshot>();

        var allTasksDone = items.Where(x => x.Type == WorkItemType.TaskItem).All(x => x.Status == WorkItemStatus.Done);
        var allBugsClosed = items.Where(x => x.Type == WorkItemType.Bug).All(x => x.Status == WorkItemStatus.Done);

        var testCases = items.Where(x => x.Type == WorkItemType.TestCase).ToList();
        var allTestsPassed =
            testCases.All(x => x.Status == WorkItemStatus.Done && x.TestCaseResult == TestCaseResult.Passed);

        return allTasksDone && allBugsClosed && allTestsPassed;
    }

    /// <summary>
    /// Applies the workflow moves safely based on linked items.
    /// Call this from Application Layer after fetching children.
    /// </summary>
    public void SyncStatusFromLinkedItems(IEnumerable<WorkItemSnapshot> linked)
    {
        if (Status == WorkItemStatus.Done) return;

        if (CanBeCompleted(linked))
        {
            if (Status != WorkItemStatus.ReadyForRelease)
                ChangeStatus(WorkItemStatus.ReadyForRelease);

            ChangeStatus(WorkItemStatus.Done);
            return;
        }

        if (CanMoveToTesting(linked))
        {
            if (Status is WorkItemStatus.InProgress or WorkItemStatus.InReview or WorkItemStatus.ReadyForDev)
                ChangeStatus(WorkItemStatus.InTesting);
        }
    }

    public static UserStory Rehydrate(
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
        DateTime? endDate,
        int storyPoints)
    {
        var us = new UserStory(id, title, createdByUserId, projectId, storyPoints);
        us.RehydrateCommon(createdAt, updatedAt, status, priority, isBlocked, effortPoints, description, acceptanceCriteria,
            assignedToUserId, sprintId, startDate, endDate);
        us.StoryPoints = storyPoints;
        return us;
    }
}