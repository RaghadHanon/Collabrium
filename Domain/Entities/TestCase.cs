using Domain.Enums;

namespace Domain.Models;

public sealed class TestCase : WorkItem
{
    public string Preconditions { get; private set; } = null!;
    public string Steps { get; private set; } = null!;
    public TestEnvironment TestEnvironment { get; private set; }
    public string ExpectedResult { get; private set; } = null!;
    public string ActualResult { get; private set; } = null!;

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
        Preconditions = preconditions;
        Steps = steps;
        TestEnvironment = env;
        ExpectedResult = expectedResult;
    }
}
