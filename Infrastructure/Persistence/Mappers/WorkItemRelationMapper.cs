using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class WorkItemRelationMapper
{
    public static WorkItemRelation ToDomain(WorkItemRelationEntity entity)
    {
        return entity is null
            ? throw new ArgumentNullException(nameof(entity))
            : WorkItemRelation.Rehydrate(
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

    public static void UpdateEntity(WorkItemRelation domain, WorkItemRelationEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        entity.SourceWorkItemId = domain.SourceWorkItemId;
        entity.TargetWorkItemId = domain.TargetWorkItemId;
        entity.RelationType = domain.RelationType;

        entity.UpdatedAt = DateTime.UtcNow;
    }
}