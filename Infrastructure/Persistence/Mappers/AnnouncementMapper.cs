using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;
public static class AnnouncementMapper
{
    public static AnnouncementEntity ToEntity(Announcement domain)
    {
        ArgumentNullException.ThrowIfNull(domain);

        return new AnnouncementEntity
        {
            Id = domain.Id,
            TenantId = domain.TenantId,
            Title = domain.Title,
            Content = domain.Content,
            Priority = domain.Priority,
            PublishAt = domain.PublishAt,
            ExpiresAt = domain.ExpiresAt,
            NotificationSent = domain.NotificationSent,
            NotificationSentAt = domain.NotificationSentAt,
            CreatedByUserId = domain.CreatedByUserId,
            CreatedAt = domain.CreatedAt,
            UpdatedAt = domain.UpdatedAt,
            Audiences = domain.Audiences
                .Select(AnnouncementAudienceMapper.ToEntity)
                .ToList()
        };
    }

    public static Announcement ToDomain(AnnouncementEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return Announcement.Rehydrate(
            id: entity.Id,
            createdAt: entity.CreatedAt,
            updatedAt: entity.UpdatedAt,
            tenantId: entity.TenantId,
            title: entity.Title,
            content: entity.Content,
            priority: entity.Priority,
            publishAt: entity.PublishAt,
            expiresAt: entity.ExpiresAt,
            createdByUserId: entity.CreatedByUserId,
            notificationSent: entity.NotificationSent,
            notificationSentAt: entity.NotificationSentAt,
            audiences: entity.Audiences.Select(AnnouncementAudienceMapper.ToDomain));
    }

    public static void UpdateEntity(AnnouncementEntity entity, Announcement domain)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(domain);

        entity.Title = domain.Title;
        entity.Content = domain.Content;
        entity.Priority = domain.Priority;
        entity.PublishAt = domain.PublishAt;
        entity.ExpiresAt = domain.ExpiresAt;
        entity.NotificationSent = domain.NotificationSent;
        entity.NotificationSentAt = domain.NotificationSentAt;
        entity.UpdatedAt = domain.UpdatedAt ?? DateTime.UtcNow;
    }
}