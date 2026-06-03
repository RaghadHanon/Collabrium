using Application.Events;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;

namespace Application.EventsHandlers;

public sealed class RemoveMemberFromTeamNotificationHandler(
    INotificationCreationService notificationCreationService
) : INotificationHandler<RemoveMemberFromTeamEvent>
{
    public async Task Handle(RemoveMemberFromTeamEvent notification, CancellationToken ct)
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
            type: NotificationType.TeamMemberRemoved,
            title: "Removed from Team",
            message: $"{notification.AssignerFullName} removed you from {notification.TeamName}.",
            triggeredByUserId: notification.AssignerUserId,
            entityType: "Team",
            entityId: notification.TeamId,
            actionUrl: "/dashboard/teams",
            cancellationToken: ct);
    }
}