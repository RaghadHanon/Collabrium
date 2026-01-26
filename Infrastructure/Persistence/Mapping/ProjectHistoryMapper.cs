using Domain.Entities;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mapping;

public static class ProjectHistoryMapper
{
    public static ProjectHistory ToDomain(ProjectHistoryEntity entity)
    {
        return entity is null
            ? throw new ArgumentNullException(nameof(entity))
            : ProjectHistory.Rehydrate(
            id: entity.Id,
            createdAt: entity.CreatedAt,
            updatedAt: entity.UpdatedAt,
            projectId: entity.ProjectId,
            changedByUserId: entity.ChangedByUserId,
            propertyName: entity.PropertyName,
            oldValue: entity.OldValue,
            newValue: entity.NewValue
        );
    }

    public static ProjectHistoryEntity ToEntity(ProjectHistory domain)
    {
        return domain is null
            ? throw new ArgumentNullException(nameof(domain))
            : new ProjectHistoryEntity
            {
                Id = domain.Id,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt,

                ProjectId = domain.ProjectId,
                ChangedByUserId = domain.ChangedByUserId,
                PropertyName = domain.PropertyName,
                OldValue = domain.OldValue,
                NewValue = domain.NewValue
            };
    }
}