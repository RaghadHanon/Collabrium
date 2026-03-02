using Domain.Enums;

namespace Application.Features.Projects.DTOs;

public sealed class ProjectDto
{
    public required string Id { get; init; }
    public required string TenantId { get; init; }

    public string? TeamId { get; init; }
    public string TeamName { get; init; } = "";

    public required string CreatedByUserId { get; init; }

    public required string Name { get; init; }
    public required string Key { get; init; }
    public string? Description { get; init; }

    public ProjectStatus Status { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}