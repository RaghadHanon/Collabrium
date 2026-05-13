using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class TopicNodeMapper
{
    public static TopicNode ToDomain(TopicNodeEntity entity)
    {
        return entity is null
            ? throw new ArgumentNullException(nameof(entity))
            : TopicNode.Rehydrate(
            id: entity.Id,
            tenantId: entity.TenantId,
            createdAt: entity.CreatedAt,
            updatedAt: entity.UpdatedAt,
            roadmapId: entity.RoadmapId,
            title: entity.Title,
            description: entity.Description,
            order: entity.Order
        );
    }

    public static TopicNodeEntity ToEntity(TopicNode domain)
    {
        return domain is null
            ? throw new ArgumentNullException(nameof(domain))
            : new TopicNodeEntity
            {
                Id = domain.Id,
                TenantId = domain.TenantId,
                RoadmapId = domain.RoadmapId,
                Title = domain.Title,
                Description = domain.Description,
                Order = domain.Order,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt
            };
    }

    public static void UpdateEntity(TopicNode domain, TopicNodeEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        entity.Title = domain.Title;
        entity.Description = domain.Description;
        entity.Order = domain.Order;
        entity.UpdatedAt = domain.UpdatedAt ?? DateTime.UtcNow;
    }
}
