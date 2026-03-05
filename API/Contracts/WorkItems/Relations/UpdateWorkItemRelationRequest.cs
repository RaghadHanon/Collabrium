using Domain.Enums;

namespace API.Contracts.WorkItems.Relations;

public class UpdateWorkItemRelationRequest
{
    public required string NewTargetWorkItemId { get; set; }
    public required WorkItemRelationType NewRelationType { get; set; }
}