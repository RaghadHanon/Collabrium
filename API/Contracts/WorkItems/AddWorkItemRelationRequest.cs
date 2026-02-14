namespace API.Contracts.WorkItems;

public class AddWorkItemRelationRequest
{
    public required string TargetWorkItemId { get; set; }
    public required string RelationType { get; set; }
}