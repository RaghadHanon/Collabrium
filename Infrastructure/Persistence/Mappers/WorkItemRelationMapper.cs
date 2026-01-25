using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class WorkItemRelationMapper
{
    public static WorkItemRelation ToDomain(WorkItemRelationEntity entity)
    {
        return entity is null
            ? throw new ArgumentNullException(nameof(entity))
            : new WorkItemRelation(entity.Id, entity.OutgoingWorkItemId, entity.IncomingWorkItemId, entity.RelationType);
    }

    public static WorkItemRelationEntity ToEntity(WorkItemRelation domain)
    {
        return domain is null
            ? throw new ArgumentNullException(nameof(domain))
            : new WorkItemRelationEntity
            {
                Id = domain.Id,
                CreatedAt = domain.CreatedAt,
                OutgoingWorkItemId = domain.OutgoingWorkItemId,
                IncomingWorkItemId = domain.IncomingWorkItemId,
                RelationType = domain.RelationType
            };
    }

    public static void UpdateEntity(WorkItemRelation domain, WorkItemRelationEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        entity.OutgoingWorkItemId = domain.OutgoingWorkItemId;
        entity.IncomingWorkItemId = domain.IncomingWorkItemId;
        entity.RelationType = domain.RelationType;
    }
}
