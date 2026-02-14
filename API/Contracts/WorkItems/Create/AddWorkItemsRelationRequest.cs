namespace API.Contracts.WorkItems.Create;

public class AddWorkItemsRelationRequest
{
    public required string TargetWorkItemId { get; set; }
    public required string RelationType { get; set; }
}