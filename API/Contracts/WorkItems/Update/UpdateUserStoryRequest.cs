namespace API.Contracts.WorkItems.Update;

public sealed class UpdateUserStoryRequest : UpdateWorkItemRequest
{
    public int? StoryPoints { get; init; }
}
