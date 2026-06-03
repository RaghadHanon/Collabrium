using Application.Events;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;

namespace Application.EventsHandlers;

public sealed class ProjectAssignedNotificationHandler(
    INotificationCreationService notificationCreationService
) : INotificationHandler<ProjectAssignedEvent>
{
    public async Task Handle(ProjectAssignedEvent notification, CancellationToken ct)
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
            type: NotificationType.ProjectAssigned,
            title: "Project Assigned",
            message: $"{notification.AssignerFullName} assigned project {notification.ProjectTitle} to {notification.TeamName}.",
            triggeredByUserId: notification.AssignerUserId,
            entityType: "Project",
            entityId: notification.ProjectId,
            actionUrl: $"/dashboard/projects/{notification.ProjectId}",
            cancellationToken: ct);
    }
}