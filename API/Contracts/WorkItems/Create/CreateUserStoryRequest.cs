namespace API.Contracts.WorkItems.Create;

/// <summary>
/// Request payload for creating a User Story work item.
/// </summary>
public class CreateUserStoryRequest : CreateWorkItemRequest
{
    /// <summary>
    /// The number of story points assigned to the user story.
    /// </summary>
    public required int StoryPoints { get; init; }
}