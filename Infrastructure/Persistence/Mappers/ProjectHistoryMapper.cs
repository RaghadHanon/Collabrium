using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class ProjectHistoryMapper
{
    public static ProjectHistory ToDomain(ProjectHistoryEntity entity)
    {
        return entity is null
            ? throw new ArgumentNullException(nameof(entity))
            : new ProjectHistory(
            id: entity.Id,
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
                ProjectId = domain.ProjectId,
                ChangedByUserId = domain.ChangedByUserId,
                PropertyName = domain.PropertyName,
                OldValue = domain.OldValue,
                NewValue = domain.NewValue
            };
    }

    public static void UpdateEntity(ProjectHistory domain, ProjectHistoryEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        entity.ProjectId = domain.ProjectId;
        entity.ChangedByUserId = domain.ChangedByUserId;
        entity.PropertyName = domain.PropertyName;
        entity.OldValue = domain.OldValue;
        entity.NewValue = domain.NewValue;
    }
}
