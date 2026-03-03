using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers
{
    public static class SubTopicsMapper
    {
        public static SubTopicEntity ToEntity(RoadmapSubTopic domain)
        {
            ArgumentNullException.ThrowIfNull(domain);
            return new SubTopicEntity
            {
                Id = domain.Id,
                TopicNodeId = domain.TopicNodeId,
                Description = domain.Description,
                Order = domain.Order,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt
            };
        }
        public static RoadmapSubTopic ToDomain(SubTopicEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return RoadmapSubTopic.Rehydrate(
                id: entity.Id,
                createdAt: entity.CreatedAt,
                updatedAt: entity.UpdatedAt,
                topicNodeId: entity.TopicNodeId,
                title: entity.Title,
                description: entity.Description,
                order: entity.Order
            );
        }

        public static void UpdateEntity(SubTopicEntity entity, RoadmapSubTopic domain)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(domain);

            entity.Title = domain.Title;
            entity.Description = domain.Description;
            entity.Order = domain.Order;
            entity.UpdatedAt = domain.UpdatedAt ?? DateTime.UtcNow;
        }
    }
}
