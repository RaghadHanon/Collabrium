using Application.Events;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;

namespace Application.EventsHandlers;
public sealed class AnnouncementCreatedNotificationHandler(
    INotificationCreationService notificationCreationService,
    IAnnouncementsReadModel announcementsReadModel,
    IUsersReadModel usersReadModel,
    ITeamsReadModel teamsReadModel
) : INotificationHandler<AnnouncementCreatedEvent>
{
    public async Task Handle(AnnouncementCreatedEvent notification, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(notification.TenantId) ||
            string.IsNullOrWhiteSpace(notification.AnnouncementId))
            return;

        var announcement = await announcementsReadModel.GetDetailsByIdAsync(
            notification.TenantId,
            notification.AnnouncementId,
            ct);

        if (announcement is null)
            return;

        if (announcement.Audiences.Count == 0)
            return;

        var recipientUserIds = new HashSet<string>();

        foreach (var audience in announcement.Audiences)
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

        recipientUserIds.Remove(announcement.CreatedByUser.Id);

        if (recipientUserIds.Count == 0)
            return;

        await notificationCreationService.CreateForUsersAsync(
            tenantId: notification.TenantId,
            recipientUserIds: recipientUserIds,
            type: NotificationType.AnnouncementCreated,
            title: "New Announcement",
            message: $"New announcement: {announcement.Title}",
            triggeredByUserId: announcement.CreatedByUser.Id,
            entityType: "Announcement",
            entityId: announcement.Id,
            actionUrl: "/community-hub/announcements",
            cancellationToken: ct);
    }
}