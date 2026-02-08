namespace API.Contracts.WorkItems.Create;

/// <summary>
/// Request payload for creating a Bug work item.
/// </summary>
public class CreateBugRequest : CreateWorkItemRequest
{
    /// <summary>
    /// Steps required to reproduce the reported bug.
    /// </summary>
    public required string StepsToReproduce { get; init; }
}