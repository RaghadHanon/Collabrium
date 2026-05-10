using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers
{
    public static class PostLikeMapper
    {
        public static PostLikeEntity ToEntity(PostLike domain)
        {
            ArgumentNullException.ThrowIfNull(domain);

            return new PostLikeEntity
            {
                Id = domain.Id,
                PostId = domain.PostId,
                CreatedByUserId = domain.CreatedByUserId,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt
            };
        }

        public static PostLike ToDomain(PostLikeEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return PostLike.Rehydrate(
                id: entity.Id,
                createdAt: entity.CreatedAt,
                updatedAt: entity.UpdatedAt,
                postId: entity.PostId,
                createdByUserId: entity.CreatedByUserId
            );
        }
    }
}
