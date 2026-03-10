using Domain.Enums;

namespace API.Contracts.WorkItems.Update;

public sealed class UpdateTestCaseRequest : UpdateWorkItemRequest
{
    public string? Preconditions { get; init; }
    public string? Steps { get; init; }
    public TestEnvironment? TestEnvironment { get; init; }
    public string? ExpectedResult { get; init; }
    public string? ActualResult { get; init; }
    public TestCaseResult? Result { get; init; }
}