namespace API.Contracts.WorkItems.Update;

public abstract class UpdateWorkItemRequest
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? AcceptanceCriteria { get; init; }
    public int? EffortPoints { get; init; }
    public string? Priority { get; init; }

    public string? AssignedToUserId { get; init; }
    public string? SprintId { get; init; }

    public bool? IsBlocked { get; init; }
}