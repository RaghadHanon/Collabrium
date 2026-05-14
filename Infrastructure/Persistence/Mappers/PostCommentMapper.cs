using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class PostCommentMapper
{
    public static PostCommentEntity ToEntity(PostComment domain)
    {
        ArgumentNullException.ThrowIfNull(domain);

        return new PostCommentEntity
        {
            Id = domain.Id,
            PostId = domain.PostId,
            Content = domain.Content,
            CreatedByUserId = domain.CreatedByUserId,
            CreatedAt = domain.CreatedAt,
            UpdatedAt = domain.UpdatedAt
        };
    }

    public static PostComment ToDomain(PostCommentEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return PostComment.Rehydrate(
            id: entity.Id,
            createdAt: entity.CreatedAt,
            updatedAt: entity.UpdatedAt,
            postId: entity.PostId,
            createdByUserId: entity.CreatedByUserId,
            content: entity.Content
        );
    }

    public static void UpdateEntity(PostCommentEntity entity, PostComment domain)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(domain);

        entity.Content = domain.Content;
        entity.UpdatedAt = domain.UpdatedAt ?? DateTime.UtcNow;
    }
}
