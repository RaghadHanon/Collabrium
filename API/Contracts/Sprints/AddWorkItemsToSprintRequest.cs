namespace API.Contracts.Sprints;

public class AddWorkItemsToSprintRequest
{
    public required string[] WorkItemIds { get; set; } = Array.Empty<string>();
}
