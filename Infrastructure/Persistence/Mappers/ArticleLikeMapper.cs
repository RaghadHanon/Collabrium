using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers
{
    public static class ArticleLikeMapper
    {
        public static ArticleLikeEntity ToEntity(ArticleLike domain)
        {
            ArgumentNullException.ThrowIfNull(domain);

            return new ArticleLikeEntity
            {
                Id = domain.Id,
                ArticleId = domain.ArticleId,
                CreatedByUserId = domain.CreatedByUserId,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt
            };
        }

        public static ArticleLike ToDomain(ArticleLikeEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return ArticleLike.Rehydrate(
                id: entity.Id,
                createdAt: entity.CreatedAt,
                updatedAt: entity.UpdatedAt,
                articleId: entity.ArticleId,
                createdByUserId: entity.CreatedByUserId
            );
        }
    }
}
