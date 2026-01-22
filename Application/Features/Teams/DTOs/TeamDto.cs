using Domain.Enums;

namespace Application.Features.Teams.DTOs;

public class TeamDto
{
    public required string Id { get; set; }
    public required string TenantId { get; set; }
    public required string TeamLeadId { get; set; }
    public required string TeamLeadName { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Department Department { get; set; }
    public int? Capacity { get; set; }
    public int Count { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CreatedAt { get; set; }
}