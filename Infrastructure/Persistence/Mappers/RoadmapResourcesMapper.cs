using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class RoadmapResourcesMapper
{
    public static ResourceEntity ToEntity(RoadmapResource domain)
    {
        ArgumentNullException.ThrowIfNull(domain);

        return new ResourceEntity
        {
            Id = domain.Id,
            SubTopicId = domain.SubTopicId,
            Title = domain.Title,
            Url = domain.Url,
            Type = domain.Type,
            Order = domain.Order,
            CreatedAt = domain.CreatedAt,
            UpdatedAt = domain.UpdatedAt
        };
    }

    public static RoadmapResource ToDomain(ResourceEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return RoadmapResource.Rehydrate(
            id: entity.Id,
            createdAt: entity.CreatedAt,
            updatedAt: entity.UpdatedAt,
            subTopicId: entity.SubTopicId,
            title: entity.Title,
            url: entity.Url,
            type: entity.Type,
            order: entity.Order
        );
    }

    public static void UpdateEntity(RoadmapResource domain, ResourceEntity entity)
    {
        ArgumentNullException.ThrowIfNull(domain);
        ArgumentNullException.ThrowIfNull(entity);

        entity.Title = domain.Title;
        entity.Url = domain.Url;
        entity.Type = domain.Type;
        entity.Order = domain.Order;
        entity.SubTopicId = domain.SubTopicId;
        entity.UpdatedAt = domain.UpdatedAt ?? DateTime.UtcNow;
    }
}
