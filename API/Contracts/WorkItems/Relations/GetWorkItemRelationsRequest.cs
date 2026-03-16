using Domain.Enums;

namespace API.Contracts.WorkItems.Relations;

public class GetWorkItemRelationsRequest
{
    public WorkItemRelationType? RelationType { get; set; }
}
