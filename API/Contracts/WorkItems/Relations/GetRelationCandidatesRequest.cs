using API.Common.EnumHandlers;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace API.Contracts.WorkItems.Relations;

public class GetRelationCandidatesRequest
{
    [ModelBinder(BinderType = typeof(SpacedEnumModelBinder<WorkItemRelationType>))]
    public WorkItemRelationType? RelationType { get; set; } = WorkItemRelationType.Parent;
}