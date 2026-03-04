using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class ProjectHistoryMapper
{
    public static ProjectHistory ToDomain(ProjectHistoryEntity entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        return ProjectHistory.Rehydrate(
            id: entity.Id,
            createdAt: entity.CreatedAt,
            updatedAt: entity.UpdatedAt,
            projectId: entity.ProjectId,
            relatedEntityId: entity.RelatedEntityId,   
            relatedEntityTitle: entity.RelatedEntityTitle,
            relatedEntityCode: entity.RelatedEntityCode,
            madeByUserId: entity.MadeByUserId,
            scope: entity.Scope,
            actionType: entity.ActionType,
            propertyName: entity.PropertyName,
            oldValue: entity.OldValue,
            newValue: entity.NewValue
        );
    }

    public static ProjectHistoryEntity ToEntity(ProjectHistory domain)
    {
        if (domain is null)
            throw new ArgumentNullException(nameof(domain));

        return new ProjectHistoryEntity
        {
            Id = domain.Id,
            CreatedAt = domain.CreatedAt,
            UpdatedAt = domain.UpdatedAt,

            ProjectId = domain.ProjectId,
            RelatedEntityId = domain.RelatedEntityId,
            RelatedEntityTitle = domain.RelatedEntityTitle,
            RelatedEntityCode = domain.RelatedEntityCode,
            MadeByUserId = domain.MadeByUserId,
            Scope = domain.Scope,
            ActionType = domain.ActionType,
            PropertyName = domain.PropertyName,
            OldValue = domain.OldValue,
            NewValue = domain.NewValue
        };
    }
}