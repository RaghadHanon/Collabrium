using Domain.Enums;

namespace Application.Features.WorkItems.DTOs.Relations;

public class WorkItemRelationDto
{
    public required string Id { get; set; }
    public RelationParticipantWorkItemDto? TargetWorkItem { get; set; }
    public WorkItemRelationType RelationType { get; set; }
}