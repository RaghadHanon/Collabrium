using Domain.Enums;

namespace API.Contracts.WorkItems;

public class GetProjectBacklogRequest
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
    /// Filters work items by assigned user identifier.
    /// </summary>
    public string? AssignedToUserId { get; init; }
    /// <summary>
    /// Filters work items by creator user identifier.
    /// </summary>
    public string? CreatedByUserId { get; init; }
    /// <summary>
    /// Filters work items by parent workItem identifier.
    /// </summary>
    public string? ParentId { get; init; }
    /// <summary>
    /// Filters work items based on whether they are assigned to a user or not.
    /// </summary>
    public bool? IsAssigned { get; set; }

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