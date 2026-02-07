using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mapping;

public static class WorkItemRelationMapper
{
    public static WorkItemRelation ToDomain(WorkItemRelationEntity entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        return WorkItemRelation.Rehydrate(
            id: entity.Id,
            createdAt: entity.CreatedAt,
            updatedAt: entity.UpdatedAt,
            sourceId: entity.SourceWorkItemId,
            targetId: entity.TargetWorkItemId,
            type: entity.RelationType
        );
    }

    public static WorkItemRelationEntity ToEntity(WorkItemRelation domain)
    {
        return domain is null
            ? throw new ArgumentNullException(nameof(domain))
            : new WorkItemRelationEntity
            {
                Id = domain.Id,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt,

                SourceWorkItemId = domain.SourceWorkItemId,
                TargetWorkItemId = domain.TargetWorkItemId,
                RelationType = domain.RelationType
            };
    }
}