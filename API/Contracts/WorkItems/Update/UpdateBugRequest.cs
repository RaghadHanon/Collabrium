namespace API.Contracts.WorkItems.Update;

public sealed class UpdateBugRequest : UpdateWorkItemRequest
{
    public string? StepsToReproduce { get; init; }
    public string? ExpectedResult { get; init; }
    public string? ActualResult { get; init; }
}
