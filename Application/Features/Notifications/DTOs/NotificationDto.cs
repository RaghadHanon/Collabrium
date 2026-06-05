using Domain.Enums;

namespace Application.Features.Notifications.DTOs;

public sealed class NotificationDto
{
    public string Id { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;
    public string RecipientUserId { get; set; } = string.Empty;

    public string? TriggeredByUserId { get; set; }
    public string? TriggeredByUserName { get; set; }

    public NotificationType Type { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public string? EntityType { get; set; }
    public string? EntityId { get; set; }

    public string? ActionUrl { get; set; }

    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }

    public DateTime CreatedAt { get; set; }
}