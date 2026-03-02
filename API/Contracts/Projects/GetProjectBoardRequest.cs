using API.Contracts.WorkItems;

namespace API.Contracts.Projects;

public class GetProjectBoardRequest 
{
    /// <summary>
    /// Filters work items by type.
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// Filters work items by status.
    /// </summary>
    public string? Status { get; init; }

    /// <summary>
    /// Filters work items by priority.
    /// </summary>
    public string? Priority { get; init; }
    /// <summary>
    /// Filters work items by their unique code.
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Filters work items by sprint identifier.
    /// </summary>
    public string? SprintId { get; init; }

    /// <summary>
    /// Filters work items by assigned user identifier.
    /// </summary>
    public string? AssignedToUserId { get; init; }

    /// <summary>
    /// Filters work items whose title contains the specified text.
    /// </summary>
    public string? TitleContains { get; init; }

    /// <summary>
    /// Filters work items created after the specified date.
    /// </summary>
    public DateTime? CreatedAfter { get; init; }

    /// <summary>
    /// Filters work items created before the specified date.
    /// </summary>
    public DateTime? CreatedBefore { get; init; }
}
