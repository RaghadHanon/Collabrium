using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class CommentMapper
{
    public static Comment ToDomain(CommentEntity entity)
    {
        return entity is null
            ? throw new ArgumentNullException(nameof(entity))
            : new Comment(entity.Id, entity.WorkItemId, entity.CreatedByUserId, entity.Content);
    }

    public static CommentEntity ToEntity(Comment domain)
    {
        return domain is null
            ? throw new ArgumentNullException(nameof(domain))
            : new CommentEntity
            {
                Id = domain.Id,
                CreatedAt = domain.CreatedAt,
                Content = domain.Content,
                CreatedByUserId = domain.CreatedByUserId,
                WorkItemId = domain.WorkItemId
            };
    }

    public static void UpdateEntity(Comment domain, CommentEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        entity.Content = domain.Content;
        entity.CreatedByUserId = domain.CreatedByUserId;
        entity.WorkItemId = domain.WorkItemId;
    }
}
