using Application.Events;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;

namespace Application.EventsHandlers;

public sealed class ToggleTeamActivationNotificationHandler(
    INotificationCreationService notificationCreationService
) : INotificationHandler<ToggleTeamActivationEvent>
{
    public async Task Handle(ToggleTeamActivationEvent notification, CancellationToken ct)
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

        var statusText = notification.IsActivated ? "activated" : "deactivated";

        await notificationCreationService.CreateForUsersAsync(
            tenantId: notification.TenantId,
            recipientUserIds: recipientUserIds,
            type: NotificationType.TeamActivationChanged,
            title: notification.IsActivated ? "Team Activated" : "Team Deactivated",
            message: $"{notification.AssignerFullName} {statusText} team {notification.TeamName}.",
            triggeredByUserId: notification.AssignerUserId,
            entityType: "Team",
            entityId: notification.TeamId,
            actionUrl: notification.IsActivated
                ? $"/dashboard/teams/{notification.TeamId}"
                : "/dashboard/teams",
            cancellationToken: ct);
    }
}