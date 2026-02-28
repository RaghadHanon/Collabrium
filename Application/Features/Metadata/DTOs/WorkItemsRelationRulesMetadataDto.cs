using Domain.Enums;

namespace Application.Features.Metadata.DTOs;

public class WorkItemsRelationRulesMetadataDto
{
    public required WorkItemType To { get; set; }
    public required WorkItemType From { get; set; }
    public required WorkItemRelationType RelationType { get; set; }
}
