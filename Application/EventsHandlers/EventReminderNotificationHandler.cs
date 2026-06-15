using Application.Events;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;

namespace Application.EventsHandlers;
public sealed class EventReminderNotificationHandler(
    INotificationCreationService notificationCreationService
) : INotificationHandler<EventReminderDueEvent>
{
    public async Task Handle(EventReminderDueEvent notification, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(notification.TenantId))
            return;

        if (notification.AttendeeUserIds.Count == 0)
            return;

        var place = notification.IsOnline
            ? "online"
            : notification.Location;

        var message = string.IsNullOrWhiteSpace(place)
            ? $"Reminder: {notification.EventTitle} starts soon at {notification.StartAt:yyyy-MM-dd HH:mm}."
            : $"Reminder: {notification.EventTitle} starts soon at {notification.StartAt:yyyy-MM-dd HH:mm} ({place}).";

        await notificationCreationService.CreateForUsersAsync(
            tenantId: notification.TenantId,
            recipientUserIds: notification.AttendeeUserIds,
            type: NotificationType.EventReminder,
            title: "Event Reminder",
            message: message,
            entityType: "Event",
            entityId: notification.EventId,
            actionUrl: "/community-hub/events",
            cancellationToken: ct);
    }
}