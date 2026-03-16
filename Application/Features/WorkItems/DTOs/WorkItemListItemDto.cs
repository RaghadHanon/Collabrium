using Application.Features.WorkItems.DTOs.Relations;
using Domain.Enums;

namespace Application.Features.WorkItems.DTOs;

public class WorkItemListItemDto
{
    public required string Id { get; set; }
    public required string ProjectId { get; set; }
    public string? SprintId { get; set; }

    public required string Title { get; set; }
    public required string Code { get; set; }

    public WorkItemType Type { get; set; }
    public WorkItemStatus Status { get; set; }
    public WorkItemPriority Priority { get; set; }
    public bool IsBlocked { get; set; }

    public string? AssignedToUserId { get; set; }

    public DateTime CreatedAt { get; set; }
    public WorkItemRelationDto? Parent { get; set; }
}