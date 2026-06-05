using Application.Events;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;

namespace Application.EventsHandlers;

public sealed class AddMembersToTeamNotificationHandler(
    INotificationCreationService notificationCreationService
) : INotificationHandler<AddMemberToTeamEvent>
{
    public async Task Handle(AddMemberToTeamEvent notification, CancellationToken ct)
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
            type: NotificationType.TeamMemberAdded,
            title: "Added to Team",
            message: $"{notification.AssignerFullName} added you to {notification.TeamName}.",
            triggeredByUserId: notification.AssignerUserId,
            entityType: "Team",
            entityId: notification.TeamId,
            actionUrl: $"/dashboard/teams/{notification.TeamId}",
            cancellationToken: ct);
    }
}