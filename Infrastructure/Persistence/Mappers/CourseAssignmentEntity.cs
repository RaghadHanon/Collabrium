using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers
{
    public static class CourseAssignmentMapper
    {
        public static CourseAssignmentEntity ToEntity(CourseAssignment domain)
        {
            ArgumentNullException.ThrowIfNull(domain);

            return new CourseAssignmentEntity
            {
                Id = domain.Id,
                TenantId = domain.TenantId,
                CourseId = domain.CourseId,
                AssignedToUserId = domain.AssignedToUserId,
                AssignedByUserId = domain.AssignedByUserId,
                AssignedAt = domain.AssignedAt,
                Status = domain.Status,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt
            };
        }

        public static CourseAssignment ToDomain(CourseAssignmentEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return CourseAssignment.Rehydrate(
                id: entity.Id,
                createdAt: entity.CreatedAt,
                updatedAt: entity.UpdatedAt,
                tenantId: entity.TenantId,
                courseId: entity.CourseId,
                assignedToUserId: entity.AssignedToUserId,
                assignedByUserId: entity.AssignedByUserId,
                assignedAt: entity.AssignedAt,
                status: entity.Status
            );
        }

        public static void UpdateEntity(CourseAssignmentEntity entity, CourseAssignment domain)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(domain);

            entity.Status = domain.Status;
            entity.UpdatedAt = domain.UpdatedAt ?? DateTime.UtcNow;
        }
    }
}

