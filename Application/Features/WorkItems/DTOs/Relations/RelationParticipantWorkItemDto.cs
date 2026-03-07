using Domain.Enums;

namespace Application.Features.WorkItems.DTOs.Relations;

public class RelationParticipantWorkItemDto
{
    public string Id { get; set; } = null!;
    public required string Code { get; set; }
    public string Title { get; set; } = null!;
    public WorkItemType Type { get; set; }
    public WorkItemStatus Status { get; set; }
}