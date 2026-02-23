using Domain.Common;
using Domain.Core;
using Domain.Enums;

namespace Domain.Models;

public sealed class TestCase : WorkItem
{
    public string Preconditions { get; private set; } = null!;
    public string Steps { get; private set; } = null!;
    public TestEnvironment TestEnvironment { get; private set; }
    public string ExpectedResult { get; private set; } = null!;
    public string? ActualResult { get; private set; }

    public TestCaseResult Result { get; private set; } = TestCaseResult.NotRun;

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

    public TestCase(
    string title,
    string createdByUserId,
    string projectId,
    string preconditions,
    string steps,
    TestEnvironment env,
    string expectedResult)
    : base(title, createdByUserId, projectId)
    {
        Type = WorkItemType.TestCase;
        Preconditions = Guard.NotNullOrWhiteSpace(preconditions, nameof(preconditions));
        Steps = Guard.NotNullOrWhiteSpace(steps, nameof(steps));
        ExpectedResult = Guard.NotNullOrWhiteSpace(expectedResult, nameof(expectedResult));
        TestEnvironment = env;
    }

    protected override bool CanReopenFromDone => true;

    protected override bool IsValidTransition(WorkItemStatus from, WorkItemStatus to)
    {
        // TestCase: ToDo -> InTesting -> Done
        // Allow Done -> InTesting (re-run)
        return from == WorkItemStatus.Done && to == WorkItemStatus.InTesting
            ? true
            : from == to
            || from switch
            {
                WorkItemStatus.ToDo => to is WorkItemStatus.InTesting,
                WorkItemStatus.InTesting => to is WorkItemStatus.Done or WorkItemStatus.ToDo,
                WorkItemStatus.Done => to is WorkItemStatus.Done,
                _ => false
            };
    }

    public void StartTesting()
    {
        Result = TestCaseResult.NotRun;
        ActualResult = null;
        ChangeStatus(WorkItemStatus.InTesting);
    }

    public void RecordOutcome(TestCaseResult result, string? actualResult, string executedByUserId, string? note = null)
    {
        Guard.Against(Status != WorkItemStatus.InTesting, "TestCase must be InTesting to record outcome.");
        Guard.Against(result == TestCaseResult.NotRun, "Outcome must be Passed/Failed/Blocked.");

        Result = result;
        ActualResult = actualResult?.Trim();

        ChangeStatus(WorkItemStatus.Done);

        if (!string.IsNullOrWhiteSpace(note))
            AddComment(Guid.NewGuid().ToString(), executedByUserId, $"Test Outcome: {result}. {note}");
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
        string? actualResult,
        TestCaseResult result)
    {
        var tc = new TestCase(id, title, createdByUserId, projectId, preconditions, steps, env, expectedResult);
        tc.RehydrateCommon(createdAt, updatedAt, status, priority, isBlocked, effortPoints, description, acceptanceCriteria,
            assignedToUserId, sprintId, startDate, endDate);
        tc.ActualResult = actualResult;
        tc.Result = result;
        return tc;
    }

    public void UpdateDefinition(string preconditions, string steps, TestEnvironment env, string expectedResult)
    {
        Preconditions = Guard.NotNullOrWhiteSpace(preconditions, nameof(preconditions));
        Steps = Guard.NotNullOrWhiteSpace(steps, nameof(steps));
        ExpectedResult = Guard.NotNullOrWhiteSpace(expectedResult, nameof(expectedResult));
        TestEnvironment = env;
        Touch();
    }
}