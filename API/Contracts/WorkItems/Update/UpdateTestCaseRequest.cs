namespace API.Contracts.WorkItems.Update;

public sealed class UpdateTestCaseRequest : UpdateWorkItemRequest
{
    public string? Preconditions { get; init; }
    public string? Steps { get; init; }
    public string? TestEnvironment { get; init; }
    public string? ExpectedResult { get; init; }
    public string? ActualResult { get; init; }
    public string? Result { get; init; }
}