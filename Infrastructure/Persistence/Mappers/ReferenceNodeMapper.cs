using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class ReferenceNodeMapper
{
    public static ReferenceNode ToDomain(ReferenceNodeEntity entity)
    {
        return entity is null
            ? throw new ArgumentNullException(nameof(entity))
            : ReferenceNode.Rehydrate(
            id: entity.Id,
            createdAt: entity.CreatedAt,
            updatedAt: entity.UpdatedAt,
            tenantId: entity.TenantId,
            roadmapId: entity.RoadmapId,
            title: entity.Title,
            description: entity.Description,
            order: entity.Order,
            refRoadmapId: entity.RefRoadmapId
        );
    }

    public static ReferenceNodeEntity ToEntity(ReferenceNode domain)
    {
        return domain is null
            ? throw new ArgumentNullException(nameof(domain))
            : new ReferenceNodeEntity
            {
                Id = domain.Id,
                TenantId = domain.TenantId,
                RoadmapId = domain.RoadmapId,
                Title = domain.Title,
                Description = domain.Description,
                Order = domain.Order,
                RefRoadmapId = domain.RefRoadmapId,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt
            };
    }

    public static void UpdateEntity(ReferenceNode domain, ReferenceNodeEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        entity.Title = domain.Title;
        entity.Description = domain.Description;
        entity.Order = domain.Order;
        entity.RefRoadmapId = domain.RefRoadmapId;
        entity.UpdatedAt = domain.UpdatedAt ?? DateTime.UtcNow;
    }
}
