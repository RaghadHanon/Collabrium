using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class NotificationMapper
{
    public static NotificationEntity ToEntity(Notification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        return notification is null
            ? throw new ArgumentNullException(nameof(notification)) :
            new NotificationEntity
        {
            Id = notification.Id,
            TenantId = notification.TenantId,
            RecipientUserId = notification.RecipientUserId,
            TriggeredByUserId = notification.TriggeredByUserId,
            Type = notification.Type,
            Title = notification.Title,
            Message = notification.Message,
            EntityType = notification.EntityType,
            EntityId = notification.EntityId,
            ActionUrl = notification.ActionUrl,
            IsRead = notification.IsRead,
            ReadAt = notification.ReadAt,
            CreatedAt = notification.CreatedAt,
            UpdatedAt = notification.UpdatedAt
        };
    }

    public static Notification ToDomain(NotificationEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return entity is null
            ? throw new ArgumentNullException(nameof(entity)):
            Notification.Rehydrate(
            id: entity.Id,
            createdAt: entity.CreatedAt,
            updatedAt: entity.UpdatedAt,
            tenantId: entity.TenantId,
            recipientUserId: entity.RecipientUserId,
            triggeredByUserId: entity.TriggeredByUserId,
            type: entity.Type,
            title: entity.Title,
            message: entity.Message,
            entityType: entity.EntityType,
            entityId: entity.EntityId,
            actionUrl: entity.ActionUrl,
            isRead: entity.IsRead,
            readAt: entity.ReadAt);
    }

    public static void UpdateEntity(Notification notification, NotificationEntity entity)
    {
        if (notification is null) throw new ArgumentNullException(nameof(notification));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        entity.TenantId = notification.TenantId;
        entity.RecipientUserId = notification.RecipientUserId;
        entity.TriggeredByUserId = notification.TriggeredByUserId;
        entity.Type = notification.Type;
        entity.Title = notification.Title;
        entity.Message = notification.Message;
        entity.EntityType = notification.EntityType;
        entity.EntityId = notification.EntityId;
        entity.ActionUrl = notification.ActionUrl;
        entity.IsRead = notification.IsRead;
        entity.ReadAt = notification.ReadAt;
        entity.CreatedAt = notification.CreatedAt;
        entity.UpdatedAt = notification.UpdatedAt;
    }
}