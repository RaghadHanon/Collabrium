using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers
{
    public static class ArticleCommentMapper
    {
        public static ArticleCommentEntity ToEntity(ArticleComment domain)
        {
            ArgumentNullException.ThrowIfNull(domain);

            return new ArticleCommentEntity
            {
                Id = domain.Id,
                ArticleId = domain.ArticleId,
                Content = domain.Content,
                CreatedByUserId = domain.CreatedByUserId,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt
            };
        }

        public static ArticleComment ToDomain(ArticleCommentEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return ArticleComment.Rehydrate(
                id: entity.Id,
                createdAt: entity.CreatedAt,
                updatedAt: entity.UpdatedAt,
                articleId: entity.ArticleId,
                content: entity.Content,
                createdByUserId: entity.CreatedByUserId
            );
        }

        public static void UpdateEntity(ArticleComment domain, ArticleCommentEntity entity)
        {
            ArgumentNullException.ThrowIfNull(domain);
            ArgumentNullException.ThrowIfNull(entity);

            entity.Content = domain.Content;
            entity.UpdatedAt = domain.UpdatedAt ?? DateTime.UtcNow;
        }
    }
}
