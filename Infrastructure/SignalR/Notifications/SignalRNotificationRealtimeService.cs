using Application.Features.Notifications.DTOs;
using Application.Interfaces.Notifications;
using Infrastructure.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.SignalR.Notifications;

public sealed class SignalRNotificationRealtimeService(
    IHubContext<NotificationHub> hubContext
) : INotificationRealtimeService
{
    private const string NotificationReceivedMethod = "NotificationReceived";
    private const string UnreadCountChangedMethod = "UnreadCountChanged";

    public Task SendToUserAsync(
        string tenantId,
        string userId,
        NotificationDto notification,
        CancellationToken cancellationToken = default)
    {
        return string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(userId)
            ? Task.CompletedTask
            : hubContext.Clients
            .Group(NotificationGroups.User(tenantId, userId))
            .SendAsync(NotificationReceivedMethod, notification, cancellationToken);
    }

    public async Task SendToUsersAsync(
        string tenantId,
        IEnumerable<NotificationDto> notifications,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || notifications is null)
            return;

        var list = notifications
            .Where(n => !string.IsNullOrWhiteSpace(n.RecipientUserId))
            .ToList();

        if (list.Count == 0)
            return;

        foreach (var notification in list)
        {
            await SendToUserAsync(
                tenantId,
                notification.RecipientUserId,
                notification,
                cancellationToken);
        }
    }

    public Task SendUnreadCountChangedAsync(
        string tenantId,
        string userId,
        int unreadCount,
        CancellationToken cancellationToken = default)
    {
        return string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(userId)
            ? Task.CompletedTask
            : hubContext.Clients
            .Group(NotificationGroups.User(tenantId, userId))
            .SendAsync(UnreadCountChangedMethod, unreadCount, cancellationToken);
    }
}