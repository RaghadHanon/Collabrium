using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers
{
    public static class CourseRatingMapper
    {
        public static CourseRatingEntity ToEntity(CourseRating domain)
        {
            ArgumentNullException.ThrowIfNull(domain);

            return new CourseRatingEntity
            {
                Id = domain.Id,
                TenantId = domain.TenantId,
                UserId = domain.UserId,
                CourseId = domain.CourseId,
                Rating = domain.Rating,
                Review = domain.Review,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt
            };
        }

        public static CourseRating ToDomain(CourseRatingEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return CourseRating.Rehydrate(
                id: entity.Id,
                createdAt: entity.CreatedAt,
                updatedAt: entity.UpdatedAt,
                tenantId: entity.TenantId,
                userId: entity.UserId,
                courseId: entity.CourseId,
                rating: entity.Rating,
                review: entity.Review
            );
        }

        public static void UpdateEntity(CourseRatingEntity entity, CourseRating domain)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(domain);

            entity.Rating = domain.Rating;
            entity.Review = domain.Review;
            entity.UpdatedAt = domain.UpdatedAt ?? DateTime.UtcNow;
        }
    }
}
