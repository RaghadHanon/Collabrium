using Application.Features.WorkItems.DTOs;
using Domain.Enums;

namespace Application.Features.Sprints.DTOs;

public class SprintDto
{
    public required string Id { get; set; }
    public required string TenantId { get; set; }
    public required string ProjectId { get; set; }

    public required string Code { get; set; }

    public string? Name { get; set; }
    public string? Goal { get; set; }

    // Planned dates
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Actual dates
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public SprintStatus Status { get; set; }

    public int WorkItemsCount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}