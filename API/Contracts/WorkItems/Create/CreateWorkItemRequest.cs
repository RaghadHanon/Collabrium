namespace API.Contracts.WorkItems.Create;

/// <summary>
/// Base request payload for creating a work item.
/// </summary>
public class CreateWorkItemRequest
{
    /// <summary>
    /// The title of the work item.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// A detailed description of the work item.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The acceptance criteria that define when the work item is considered complete.
    /// </summary>
    public string? AcceptanceCriteria { get; init; }

    /// <summary>
    /// The estimated effort required to complete the work item.
    /// </summary>
    public int EffortPoints { get; init; }

    /// <summary>
    /// The priority of the work item (e.g., Low, Medium, High).
    /// </summary>
    public required string Priority { get; init; }
}