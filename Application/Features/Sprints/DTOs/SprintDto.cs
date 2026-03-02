using Domain.Enums;

namespace Application.Features.Sprints.DTOs;

public class SprintDto
{
    public required string Id { get; set; }
    public required string TenantId { get; set; }
    public required string ProjectId { get; set; }

    public required string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SprintStatus Status { get; set; }

    public int WorkItemsCount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}