using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers
{
    public static class UserCourseMapper
    {
        public static UserCourseEntity ToEntity(UserCourse domain)
        {
            ArgumentNullException.ThrowIfNull(domain);

            return new UserCourseEntity
            {
                Id = domain.Id,
                TenantId = domain.TenantId,
                UserId = domain.UserId,
                CourseId = domain.CourseId,
                Status = domain.Status,
                IsSaved = domain.IsSaved,
                StartedAt = domain.StartedAt,
                CompletedAt = domain.CompletedAt,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt
            };
        }

        public static UserCourse ToDomain(UserCourseEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return UserCourse.Rehydrate(
                id: entity.Id,
                createdAt: entity.CreatedAt,
                updatedAt: entity.UpdatedAt,
                tenantId: entity.TenantId,
                userId: entity.UserId,
                courseId: entity.CourseId,
                status: entity.Status,
                isSaved: entity.IsSaved,
                startedAt: entity.StartedAt,
                completedAt: entity.CompletedAt
            );
        }

        public static void UpdateEntity(UserCourseEntity entity, UserCourse domain)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(domain);

            entity.Status = domain.Status;
            entity.IsSaved = domain.IsSaved;
            entity.StartedAt = domain.StartedAt;
            entity.CompletedAt = domain.CompletedAt;
            entity.UpdatedAt = domain.UpdatedAt ?? DateTime.UtcNow;
        }
    }
}

