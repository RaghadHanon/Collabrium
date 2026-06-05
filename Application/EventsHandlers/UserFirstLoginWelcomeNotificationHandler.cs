using Application.Events;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;

namespace Application.EventsHandlers;

public sealed class UserFirstLoginWelcomeNotificationHandler(
    INotificationCreationService notificationCreationService
) : INotificationHandler<UserFirstLoginWelcomeEvent>
{
    public async Task Handle(
        UserFirstLoginWelcomeEvent notification,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(notification.TenantId) ||
            string.IsNullOrWhiteSpace(notification.UserId))
            return;

        await notificationCreationService.CreateForUsersAsync(
            tenantId: notification.TenantId,
            recipientUserIds: [notification.UserId],
            type: NotificationType.Welcome,
            title: "Welcome to Collabrium",
            message: $"Welcome {notification.FullName}, we’re glad to have you in Collabrium.",
            triggeredByUserId: null,
            entityType: "System",
            entityId: null,
            actionUrl: null,
            cancellationToken: ct);
    }
}