using Domain.Core;
using Domain.Enums;

namespace Domain.Models;

public sealed class Notification : Entity
{
    public string TenantId { get; private set; } = string.Empty;

    public string RecipientUserId { get; private set; } = string.Empty;
    public string? TriggeredByUserId { get; private set; }

    public NotificationType Type { get; private set; }

    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;

    public string? EntityType { get; private set; }
    public string? EntityId { get; private set; }

    public string? ActionUrl { get; private set; }

    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }

    private Notification() { }

    public Notification(
        string id,
        string tenantId,
        string recipientUserId,
        NotificationType type,
        string title,
        string message,
        string? triggeredByUserId = null,
        string? entityType = null,
        string? entityId = null,
        string? actionUrl = null)
        : base(id)
    {
        TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId)).Trim();
        RecipientUserId = Guard.NotNullOrWhiteSpace(recipientUserId, nameof(recipientUserId)).Trim();

        Type = type;

        Title = Guard.NotNullOrWhiteSpace(title, nameof(title)).Trim();
        Message = Guard.NotNullOrWhiteSpace(message, nameof(message)).Trim();

        TriggeredByUserId = string.IsNullOrWhiteSpace(triggeredByUserId)
            ? null
            : triggeredByUserId.Trim();

        EntityType = string.IsNullOrWhiteSpace(entityType)
            ? null
            : entityType.Trim();

        EntityId = string.IsNullOrWhiteSpace(entityId)
            ? null
            : entityId.Trim();

        ActionUrl = string.IsNullOrWhiteSpace(actionUrl)
            ? null
            : actionUrl.Trim();

        IsRead = false;
        ReadAt = null;
    }

    public static Notification Rehydrate(
        string id,
        DateTime createdAt,
        DateTime? updatedAt,
        string tenantId,
        string recipientUserId,
        string? triggeredByUserId,
        NotificationType type,
        string title,
        string message,
        string? entityType,
        string? entityId,
        string? actionUrl,
        bool isRead,
        DateTime? readAt)
    {
        var notification = new Notification(
            id,
            tenantId,
            recipientUserId,
            type,
            title,
            message,
            triggeredByUserId,
            entityType,
            entityId,
            actionUrl);

        notification.SetTimestamps(createdAt, updatedAt);

        notification.IsRead = isRead;
        notification.ReadAt = readAt;

        return notification;
    }

    public void MarkAsRead()
    {
        if (IsRead)
            return;

        IsRead = true;
        ReadAt = DateTime.UtcNow;
        Touch();
    }

    public void MarkAsUnread()
    {
        if (!IsRead)
            return;

        IsRead = false;
        ReadAt = null;
        Touch();
    }
}