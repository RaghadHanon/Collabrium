using Application.Events;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;

namespace Application.EventsHandlers;
public sealed class AnnouncementCreatedNotificationHandler(
    INotificationCreationService notificationCreationService,
    IUsersReadModel usersReadModel,
    ITeamsReadModel teamsReadModel
) : INotificationHandler<AnnouncementCreatedEvent>
{
    public async Task Handle(AnnouncementCreatedEvent notification, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(notification.TenantId))
            return;

        if (notification.Audiences.Count == 0)
            return;

        var recipientUserIds = new HashSet<string>();

        foreach (var audience in notification.Audiences)
        {
            if (audience.Type == AnnouncementAudienceType.Everyone)
            {
                var userIds = await usersReadModel.GetTenantUserIdsAsync(
                    notification.TenantId,
                    ct);

                foreach (var id in userIds)
                    recipientUserIds.Add(id);
            }
            else if (audience.Type == AnnouncementAudienceType.Team &&
                     !string.IsNullOrWhiteSpace(audience.TeamId))
            {
                var memberIds = await teamsReadModel.GetTeamMemberIdsAsync(
                    notification.TenantId,
                    audience.TeamId,
                    ct);

                foreach (var id in memberIds)
                    recipientUserIds.Add(id);
            }
            else if (audience.Type == AnnouncementAudienceType.Role &&
                     !string.IsNullOrWhiteSpace(audience.RoleName))
            {
                var userIds = await usersReadModel.GetUserIdsByRoleAsync(
                    notification.TenantId,
                    audience.RoleName,
                    ct);

                foreach (var id in userIds)
                    recipientUserIds.Add(id);
            }
        }

        recipientUserIds.Remove(notification.CreatedByUserId);

        if (recipientUserIds.Count == 0)
            return;

        await notificationCreationService.CreateForUsersAsync(
            tenantId: notification.TenantId,
            recipientUserIds: recipientUserIds,
            type: NotificationType.AnnouncementCreated,
            title: "New Announcement",
            message: $"New announcement: {notification.Title}",
            triggeredByUserId: notification.CreatedByUserId,
            entityType: "Announcement",
            entityId: notification.AnnouncementId,
            actionUrl: "/community-hub/announcements",
            cancellationToken: ct);
    }
}