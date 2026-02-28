using Domain.Common;
using Domain.Enums;

namespace Domain.Models;

public sealed class UserStory : WorkItem
{
    public int StoryPoints { get; private set; }

    private UserStory() { } // EF

    public UserStory(string id, string title, string createdByUserId, string projectId, int storyPoints)
        : base(id, title, createdByUserId, projectId)
    {
        Type = WorkItemType.UserStory;
        Estimate(storyPoints);
    }

    public UserStory(string title, string createdByUserId, string projectId, int storyPoints)
        : base(title, createdByUserId, projectId)
    {
        Type = WorkItemType.UserStory;
        Estimate(storyPoints);
    }

    protected override bool CanReopenFromDone => true;

    public void Estimate(int points)
    {
        Guard.Against(points < 0, "StoryPoints cannot be negative.");
        Guard.Against(points > 100, "StoryPoints is too high.");
        StoryPoints = points;
        Touch();
    }

    public override void ChangeStatusWithValidation(
         WorkItemStatus newStatus,
         IEnumerable<WorkItemSnapshot>? linked)
    {
        ValidateStatusGate(newStatus, linked);
        base.ChangeStatus(newStatus);
    }

    private void ValidateStatusGate(WorkItemStatus target, IEnumerable<WorkItemSnapshot>? linked)
    {
        if (target is not (WorkItemStatus.InTesting or WorkItemStatus.ReadyForRelease or WorkItemStatus.Done))
            return;

        var items = linked?.ToList() ?? new List<WorkItemSnapshot>();

        bool allTasksDone =
            items.Where(x => x.Type == WorkItemType.TaskItem)
                 .All(x => x.Status == WorkItemStatus.Done);

        bool noOpenBugs =
            items.Where(x => x.Type == WorkItemType.Bug)
                 .All(x => x.Status == WorkItemStatus.Done);

        var testCases = items.Where(x => x.Type == WorkItemType.TestCase).ToList();

        bool allTestsPassed =
            testCases.Count > 0 &&
            testCases.All(x => x.Status == WorkItemStatus.Done &&
                               x.TestCaseResult == TestCaseResult.Passed);

        if (target == WorkItemStatus.InTesting)
        {
            Guard.Against(!allTasksDone,
                "Cannot move UserStory to InTesting until all linked Tasks are Done.");

            Guard.Against(!noOpenBugs,
                "Cannot move UserStory to InTesting while there are open Bugs.");

            return;
        }

        if (target == WorkItemStatus.ReadyForRelease)
        {
            Guard.Against(!allTasksDone,
                "Cannot move UserStory to ReadyForRelease until all linked Tasks are Done.");

            Guard.Against(!noOpenBugs,
                "Cannot move UserStory to ReadyForRelease while there are open Bugs.");

            Guard.Against(!allTestsPassed,
                "Cannot move UserStory to ReadyForRelease until all linked TestCases are Passed.");

            return;
        }

        if (target == WorkItemStatus.Done)
        {
            Guard.Against(!allTasksDone,
                "Cannot complete UserStory until all linked Tasks are Done.");

            Guard.Against(!noOpenBugs,
                "Cannot complete UserStory while there are open Bugs.");

            Guard.Against(!allTestsPassed,
                "Cannot complete UserStory until all linked TestCases are Passed.");
        }
    }

    public static UserStory Rehydrate(
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
        DateTime? endDate,
        int storyPoints)
    {
        var us = new UserStory(id, title, createdByUserId, projectId, storyPoints);

        us.RehydrateCommon(
            createdAt, updatedAt, type,
            status, priority, isBlocked, effortPoints,
            title, description, acceptanceCriteria,
            assignedToUserId, createdByUserId, sprintId, projectId,
            startDate, endDate);

        us.StoryPoints = storyPoints;
        return us;
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
            EndDate,
            StoryPoints
        );

        return clone;
    }
}