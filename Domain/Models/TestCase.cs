using Domain.Common;
using Domain.Enums;

namespace Domain.Models;

public sealed class TestCase : WorkItem
{
    public string Preconditions { get; private set; } = null!;
    public string Steps { get; private set; } = null!;
    public TestEnvironment TestEnvironment { get; private set; }
    public string ExpectedResult { get; private set; } = null!;
    public string? ActualResult { get; private set; }

    private TestCase() { }

    public TestCase(
        string id,
        string title,
        string createdByUserId,
        string projectId,
        string preconditions,
        string steps,
        TestEnvironment env,
        string expectedResult)
        : base(id, title, createdByUserId, projectId)
    {
        Type = WorkItemType.TestCase;
        Preconditions = Guard.NotNullOrWhiteSpace(preconditions, nameof(preconditions));
        Steps = Guard.NotNullOrWhiteSpace(steps, nameof(steps));
        ExpectedResult = Guard.NotNullOrWhiteSpace(expectedResult, nameof(expectedResult));
        TestEnvironment = env;
    }

    public static TestCase Rehydrate(
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
        string preconditions,
        string steps,
        TestEnvironment env,
        string expectedResult,
        string? actualResult)
    {
        var tc = new TestCase(id, title, createdByUserId, projectId, preconditions, steps, env, expectedResult);
        tc.RehydrateCommon(createdAt, updatedAt, status, priority, isBlocked, effortPoints, description, acceptanceCriteria,
            assignedToUserId, sprintId, startDate, endDate);
        tc.ActualResult = actualResult;
        return tc;
    }

    public void RecordActualResult(string? actual)
    {
        ActualResult = actual?.Trim();
        Touch();
    }
}