using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers
{
    public static class RoadmapNodeMapper
    {
        public static RoadmapNode ToDomain(RoadmapNodeEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            if (entity is TopicNodeEntity topic)
            {
                return TopicNode.Rehydrate(
                    id: topic.Id,
                    createdAt: topic.CreatedAt,
                    updatedAt: topic.UpdatedAt,
                    roadmapId: topic.RoadmapId,
                    title: topic.Title,
                    description: topic.Description,
                    order: topic.Order
                );
            }

            if (entity is ReferenceNodeEntity reference)
            {
                return ReferenceNode.Rehydrate(
                    id: reference.Id,
                    createdAt: reference.CreatedAt,
                    updatedAt: reference.UpdatedAt,
                    roadmapId: reference.RoadmapId,
                    title: reference.Title,
                    description: reference.Description,
                    order: reference.Order,
                    refRoadmapId: reference.RefRoadmapId
                );
            }

            throw new InvalidOperationException("Unknown RoadmapNodeEntity type.");
        }
        public static RoadmapNodeEntity ToEntity(RoadmapNode domain)
        {
            ArgumentNullException.ThrowIfNull(domain);

            if (domain is TopicNode topic)
            {
                return new TopicNodeEntity
                {
                    Id = topic.Id,
                    RoadmapId = topic.RoadmapId,
                    Title = topic.Title,
                    Description = topic.Description,
                    Order = topic.Order,
                    CreatedAt = topic.CreatedAt,
                    UpdatedAt = topic.UpdatedAt
                };
            }

            if (domain is ReferenceNode reference)
            {
                return new ReferenceNodeEntity
                {
                    Id = reference.Id,
                    RoadmapId = reference.RoadmapId,
                    Title = reference.Title,
                    Description = reference.Description,
                    Order = reference.Order,
                    RefRoadmapId = reference.RefRoadmapId,
                    CreatedAt = reference.CreatedAt,
                    UpdatedAt = reference.UpdatedAt
                };
            }

            throw new InvalidOperationException("Unknown RoadmapNode domain type.");
        }
        public static void UpdateEntity(RoadmapNode domain, RoadmapNodeEntity entity)
        {
            ArgumentNullException.ThrowIfNull(domain);
            ArgumentNullException.ThrowIfNull(entity);

            // مشتركات
            entity.Title = domain.Title;
            entity.Description = domain.Description;
            entity.Order = domain.Order;
            entity.UpdatedAt = domain.UpdatedAt ?? DateTime.UtcNow;

            // خاص بالريفيرنس فقط
            if (domain is ReferenceNode refDomain)
            {
                if (entity is not ReferenceNodeEntity refEntity)
                    throw new InvalidOperationException("Cannot update ReferenceNode into non-reference entity.");

                refEntity.RefRoadmapId = refDomain.RefRoadmapId;
            }
            else
            {
                // لو كان TopicNode بس تأكدي إن الـ entity كمان TopicNodeEntity
                if (entity is not TopicNodeEntity)
                    throw new InvalidOperationException("Cannot update TopicNode into non-topic entity.");
            }
        }
    }

}


