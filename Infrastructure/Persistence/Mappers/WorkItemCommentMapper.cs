using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class WorkItemCommentMapper
{
    public static WorkItemComment ToDomain(WorkItemCommentEntity entity)
    {
        return entity is null
            ? throw new ArgumentNullException(nameof(entity))
            : WorkItemComment.Rehydrate(
            id: entity.Id,
            createdAt: entity.CreatedAt,
            updatedAt: entity.UpdatedAt,
            createdByUserId: entity.CreatedByUserId,
            content: entity.Content
        );
    }

    public static WorkItemCommentEntity ToEntity(WorkItemComment domain, string workItemId)
    {
        return domain is null
            ? throw new ArgumentNullException(nameof(domain))
            : string.IsNullOrWhiteSpace(workItemId)
            ? throw new ArgumentNullException(nameof(workItemId))
            : new WorkItemCommentEntity
            {
                Id = domain.Id,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt,

                WorkItemId = workItemId,
                CreatedByUserId = domain.CreatedByUserId,
                Content = domain.Content
            };
    }

    public static void UpdateEntity(WorkItemComment domain, WorkItemCommentEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        entity.Content = domain.Content;
        entity.UpdatedAt = domain.UpdatedAt ?? DateTime.UtcNow;
    }
}