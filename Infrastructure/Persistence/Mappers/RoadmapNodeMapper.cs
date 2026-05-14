using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class RoadmapNodeMapper
{
    public static RoadmapNode ToDomain(RoadmapNodeEntity entity)
    {
        return entity is null
            ? throw new ArgumentNullException(nameof(entity))
            : entity switch
            {
                TopicNodeEntity t => TopicNodeMapper.ToDomain(t),
                ReferenceNodeEntity r => ReferenceNodeMapper.ToDomain(r),
                _ => throw new InvalidOperationException($"Unsupported node entity type: {entity.GetType().Name}")
            };
    }

    public static RoadmapNodeEntity ToEntity(RoadmapNode domain)
    {
        return domain is null
            ? throw new ArgumentNullException(nameof(domain))
            : domain switch
            {
                TopicNode t => TopicNodeMapper.ToEntity(t),
                ReferenceNode r => ReferenceNodeMapper.ToEntity(r),
                _ => throw new InvalidOperationException($"Unsupported node domain type: {domain.GetType().Name}")
            };
    }

    public static void UpdateEntity(RoadmapNode domain, RoadmapNodeEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        switch (domain)
        {
            case TopicNode t when entity is TopicNodeEntity tEntity:
                TopicNodeMapper.UpdateEntity(t, tEntity);
                break;

            case ReferenceNode r when entity is ReferenceNodeEntity rEntity:
                ReferenceNodeMapper.UpdateEntity(r, rEntity);
                break;

            default:
                throw new InvalidOperationException("Domain/entity node type mismatch.");
        }
    }
}


