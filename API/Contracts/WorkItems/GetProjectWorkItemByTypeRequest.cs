namespace API.Contracts.WorkItems;

public class GetProjectWorkItemByTypeRequest: GetSprintUserStoriesRequest
{
    /// <summary>
    /// Filters work items by sprint identifier.
    /// </summary>
    public string? SprintId { get; init; }
}