using Domain.Enums;

namespace API.Contracts.WorkItems;

public class UpdateWorkItemRelationRequest
{
    public required string NewTargetWorkItemId { get; set; }
    public required WorkItemRelationType NewRelationType { get; set; }
}