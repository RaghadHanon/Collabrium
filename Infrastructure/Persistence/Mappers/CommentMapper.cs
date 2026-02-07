using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mapping;

public static class CommentMapper
{
    public static Comment ToDomain(CommentEntity entity)
    {
        return entity is null
            ? throw new ArgumentNullException(nameof(entity))
            : Comment.Rehydrate(
            id: entity.Id,
            createdAt: entity.CreatedAt,
            updatedAt: entity.UpdatedAt,
            createdByUserId: entity.CreatedByUserId,
            content: entity.Content
        );
    }

    public static CommentEntity ToEntity(Comment domain, string workItemId)
    {
        return domain is null
            ? throw new ArgumentNullException(nameof(domain))
            : string.IsNullOrWhiteSpace(workItemId)
            ? throw new ArgumentNullException(nameof(workItemId))
            : new CommentEntity
            {
                Id = domain.Id,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt,

                WorkItemId = workItemId,
                CreatedByUserId = domain.CreatedByUserId,
                Content = domain.Content
            };
    }

    public static void UpdateEntity(Comment domain, CommentEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        entity.Content = domain.Content;
        entity.UpdatedAt = domain.UpdatedAt ?? DateTime.UtcNow;
    }
}