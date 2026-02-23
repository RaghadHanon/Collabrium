namespace API.Contracts.WorkItems.Relations;

public class UpdateWorkItemRelationRequest
{
    public required string NewTargetWorkItemId { get; set; }
    public required string NewRelationType { get; set; }
}