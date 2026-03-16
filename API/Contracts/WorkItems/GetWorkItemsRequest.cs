using Domain.Enums;

namespace API.Contracts.WorkItems;

/// <summary>
/// Represents filtering and pagination parameters for retrieving work items.
/// </summary>
public class GetWorkItemsRequest
{
    /// <summary>
    /// Filters work items by type.
    /// </summary>
    public WorkItemType? Type { get; init; }

    /// <summary>
    /// Filters work items by status.
    /// </summary>
    public WorkItemStatus? Status { get; init; }

    /// <summary>
    /// Filters work items by priority.
    /// </summary>
    public WorkItemPriority? Priority { get; init; }

    /// <summary>
    /// Filters work items by sprint identifier.
    /// </summary>
    public string? SprintId { get; init; }

    /// <summary>
    /// Filters work items by assigned user identifier.
    /// </summary>
    public string? AssignedToUserId { get; init; }
    /// <summary>
    /// Filters work items by creator user identifier.
    /// </summary>
    public string? CreatedByUserId { get; init; }

    /// <summary>
    /// Filters work items whose title contains the specified text.
    /// </summary>
    public string? TitleContains { get; init; }
    /// <summary>
    /// Filters work items by their unique code.
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Filters work items created after the specified date.
    /// </summary>
    public DateTime? CreatedAfter { get; init; }

    /// <summary>
    /// Filters work items created before the specified date.
    /// </summary>
    public DateTime? CreatedBefore { get; init; }

    /// <summary>
    /// The maximum number of items to return per page.
    /// </summary>
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// Cursor used for pagination.
    /// </summary>
    public DateTime? Cursor { get; init; }
}