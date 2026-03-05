using Domain.Common;
using Domain.Enums;

namespace Domain.Models;

public class Bug : WorkItem
{
    public string StepsToReproduce { get; private set; } = null!;
    public string? ActualResult { get; private set; }
    public string? ExpectedResult { get; private set; }

    private Bug() { }

    public Bug(string id, string title, string code, string createdByUserId, string projectId, string stepsToReproduce)
        : base(id, title, code, createdByUserId, projectId)
    {
        Type = WorkItemType.Bug;
        StepsToReproduce = Guard.NotNullOrWhiteSpace(stepsToReproduce, nameof(stepsToReproduce));
    }

    public Bug(string title, string code, string createdByUserId, string projectId, string actualResult, string expectedResult, string stepsToReproduce)
           : base(title, code, createdByUserId, projectId)
    {
        Type = WorkItemType.Bug;
        StepsToReproduce = Guard.NotNullOrWhiteSpace(stepsToReproduce, nameof(stepsToReproduce));
        UpdateRepro(stepsToReproduce, actualResult, expectedResult);
    }

    public Bug(string title, string code, string createdByUserId, string projectId, string stepsToReproduce)
       : base(title, code, createdByUserId, projectId)
    {
        Type = WorkItemType.Bug;
        StepsToReproduce = Guard.NotNullOrWhiteSpace(stepsToReproduce, nameof(stepsToReproduce));
    }
    protected override bool CanReopenFromDone => true;

    protected override bool IsValidTransition(WorkItemStatus from, WorkItemStatus to)
    {
        // Bug workflow: ToDo -> ReadyForDev -> InProgress -> InReview -> InTesting -> Done
        // Allow Done -> InProgress (Reopen)
        return from == to
            || to == WorkItemStatus.ToDo
            || from switch
            {
                WorkItemStatus.ToDo => to is WorkItemStatus.ReadyForDev,
                WorkItemStatus.ReadyForDev => to is WorkItemStatus.InProgress or WorkItemStatus.ToDo,
                WorkItemStatus.InProgress => to is WorkItemStatus.InTesting,
                WorkItemStatus.InReview => to is WorkItemStatus.InTesting or WorkItemStatus.InProgress,
                WorkItemStatus.InTesting => to is WorkItemStatus.Done or WorkItemStatus.InProgress or WorkItemStatus.InReview,
                WorkItemStatus.Done => to is WorkItemStatus.InTesting or WorkItemStatus.InProgress or WorkItemStatus.InReview,
                _ => false
            };
    }

    public void Reopen(string reopenedByUserId, string? note = null)
    {
        Guard.Against(!IsDone, "Only Done bug can be reopened.");
        ChangeStatus(WorkItemStatus.InProgress);
        if (!string.IsNullOrWhiteSpace(note))
            AddComment(Guid.NewGuid().ToString(), reopenedByUserId, $"Reopened: {note}");
    }

    public static Bug Rehydrate(
        string id,
        DateTime createdAt,
        DateTime? updatedAt,
        string title,
        string code,
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
        string stepsToReproduce,
        string? expectedResult,
        string? actualResult)
    {
        var b = new Bug(id, title, code, createdByUserId, projectId, stepsToReproduce);
        b.RehydrateCommon(createdAt, updatedAt, type, status, priority, isBlocked, effortPoints, title, code, description, acceptanceCriteria,
            assignedToUserId, createdByUserId, sprintId, projectId, startDate, endDate);

        b.ExpectedResult = expectedResult;
        b.ActualResult = actualResult;

        return b;
    }

    public override WorkItem Clone()
    {
        var clone = Rehydrate(
            Id,
            CreatedAt,
            UpdatedAt,
            Title,
            Code,
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
            StepsToReproduce,
            ExpectedResult,
            ActualResult
        );

        return clone;
    }

    public void UpdateRepro(string steps, string? expected, string? actual)
    {
        StepsToReproduce = Guard.NotNullOrWhiteSpace(steps, nameof(steps));
        ExpectedResult = expected?.Trim();
        ActualResult = actual?.Trim();
        Touch();
    }
}