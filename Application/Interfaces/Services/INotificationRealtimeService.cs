using Application.Features.Notifications.DTOs;

namespace Application.Interfaces.Notifications;

public interface INotificationRealtimeService
{
    Task SendToUserAsync(
        string tenantId,
        string userId,
        NotificationDto notification,
        CancellationToken cancellationToken = default);

    Task SendToUsersAsync(
        string tenantId,
        IEnumerable<NotificationDto> notifications,
        CancellationToken cancellationToken = default);

    Task SendUnreadCountChangedAsync(
        string tenantId,
        string userId,
        int unreadCount,
        CancellationToken cancellationToken = default);
}