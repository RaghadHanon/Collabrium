using Domain.Enums;

namespace Application.Features.WorkItems.DTOs;

public sealed class WorkItemDto
{
    public required string Id { get; init; }
    public required string ProjectId { get; init; }

    public required string Title { get; init; }
    public WorkItemStatus Status { get; init; }
    public WorkItemPriority Priority { get; init; }

    public DateTime CreatedAt { get; init; }
    public string? AssignedToUserId { get; init; }
}