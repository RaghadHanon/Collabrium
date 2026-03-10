using Domain.Enums;

namespace API.Contracts.WorkItems.Relations;

public class AddWorkItemRelationRequest
{
    public required string TargetWorkItemId { get; set; }
    public required WorkItemRelationType RelationType { get; set; }
}