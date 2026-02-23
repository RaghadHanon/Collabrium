using Application.Common.DTOs;
using Domain.Enums;

namespace Application.Features.WorkItems.DTOs;

public sealed class WorkItemsSearchAndFilterParametersDto : QueryParametersDto<DateTime?>
{
    public string? TenantId { get; set; }
    public string? ProjectId { get; set; }

    public WorkItemType? Type { get; set; }
    public WorkItemStatus? Status { get; set; }
    public WorkItemPriority? Priority { get; set; }

    public string? SprintId { get; set; }
    public string? AssignedToUserId { get; set; }

    public string? TitleContains { get; set; }

    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
}