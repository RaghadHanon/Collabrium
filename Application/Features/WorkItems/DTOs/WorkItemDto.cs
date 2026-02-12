using Domain.Enums;

namespace Application.Features.WorkItems.DTOs;

public class WorkItemDto
{
    public required string Id { get; set; }

    public required string ProjectId { get; set; }
    public string? SprintId { get; set; }

    public required string Title { get; set; }
    public string? Description { get; set; }
    public string? AcceptanceCriteria { get; set; }

    public int EffortPoints { get; set; }
    public WorkItemType Type { get; set; }
    public WorkItemStatus Status { get; set; }
    public WorkItemPriority Priority { get; set; }
    public bool IsBlocked { get; set; }

    public string CreatedByUserId { get; set; } = null!;
    public string? AssignedToUserId { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<WorkItemStatus>? AllowedStatuses { get; set; } = new List<WorkItemStatus>();
}