using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public sealed class Bug : WorkItem
{
    public string StepsToReproduce { get; private set; } = null!;
    public string? ActualResult { get; private set; }
    public string? ExpectedResult { get; private set; }

    private Bug() { }

    public Bug(string id, string title, string createdByUserId, string projectId, string stepsToReproduce)
        : base(id, title, createdByUserId, projectId)
    {
        Type = WorkItemType.Bug;
        StepsToReproduce = Guard.NotNullOrWhiteSpace(stepsToReproduce, nameof(stepsToReproduce));
    }

    public static Bug Rehydrate(
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
        string stepsToReproduce,
        string? expectedResult,
        string? actualResult)
    {
        var b = new Bug(id, title, createdByUserId, projectId, stepsToReproduce);
        b.RehydrateCommon(createdAt, updatedAt, status, priority, isBlocked, effortPoints, description, acceptanceCriteria,
            assignedToUserId, sprintId, startDate, endDate);

        b.ExpectedResult = expectedResult;
        b.ActualResult = actualResult;

        return b;
    }

    public void UpdateRepro(string steps, string? expected, string? actual)
    {
        StepsToReproduce = Guard.NotNullOrWhiteSpace(steps, nameof(steps));
        ExpectedResult = expected?.Trim();
        ActualResult = actual?.Trim();
        Touch();
    }
}