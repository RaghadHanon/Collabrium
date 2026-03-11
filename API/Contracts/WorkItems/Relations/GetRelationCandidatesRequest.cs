using Domain.Enums;

namespace API.Contracts.WorkItems.Relations;

public class GetRelationCandidatesRequest
{
    public WorkItemRelationType? RelationType { get; set; } = WorkItemRelationType.Parent;
}