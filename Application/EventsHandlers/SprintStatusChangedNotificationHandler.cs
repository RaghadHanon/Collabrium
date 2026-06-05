using Application.Events;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;

namespace Application.EventsHandlers;

public sealed class SprintStatusChangedNotificationHandler(
    INotificationCreationService notificationCreationService
) : INotificationHandler<SprintStatusChangedEvent>
{
    public async Task Handle(SprintStatusChangedEvent notification, CancellationToken ct)
    {
        if (notification.TeamMembers.Count == 0)
            return;

        var recipientUserIds = notification.TeamMembers
            .Select(m => m.Id)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        if (recipientUserIds.Count == 0)
            return;

        await notificationCreationService.CreateForUsersAsync(
            tenantId: notification.TenantId,
            recipientUserIds: recipientUserIds,
            type: NotificationType.SprintStatusChanged,
            title: "Sprint Status Changed",
            message: $"{notification.AssignerFullName} changed sprint {notification.SprintName} status to {notification.Status}.",
            triggeredByUserId: notification.AssignerUserId,
            entityType: "Sprint",
            entityId: notification.SprintId,
            actionUrl: $"/projects/{notification.ProjectId}/board/{notification.SprintId}",
            cancellationToken: ct);
    }
}