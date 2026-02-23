namespace API.Contracts.WorkItems.Create;

/// <summary>
/// Request payload for creating a Test Case work item.
/// </summary>
public class CreateTestCaseRequest : CreateWorkItemRequest
{
    /// <summary>
    /// Preconditions that must be satisfied before executing the test case.
    /// </summary>
    public required string Preconditions { get; init; }

    /// <summary>
    /// The steps required to execute the test case.
    /// </summary>
    public required string Steps { get; init; }

    /// <summary>
    /// The environment in which the test case should be executed.
    /// </summary>
    public required string TestEnvironment { get; init; }

    /// <summary>
    /// The expected result after executing the test case.
    /// </summary>
    public required string ExpectedResult { get; init; }
}