using Application.Events;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;

namespace Application.EventsHandlers;

public sealed class CourseAssignedToTeamNotificationHandler(
    INotificationCreationService notificationCreationService
) : INotificationHandler<CourseAssignedToTeamEvent>
{
    public async Task Handle(CourseAssignedToTeamEvent notification, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(notification.TenantId))
            return;

        if (notification.Members.Count == 0)
            return;

        var recipientUserIds = notification.Members
            .Select(m => m.Id)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        if (recipientUserIds.Count == 0)
            return;

        await notificationCreationService.CreateForUsersAsync(
            tenantId: notification.TenantId,
            recipientUserIds: recipientUserIds,
            type: NotificationType.CourseAssigned,
            title: "Course Assigned",
            message: $"{notification.AssignerFullName} assigned course {notification.CourseTitle} to team {notification.TeamName}.",
            triggeredByUserId: notification.AssignerUserId,
            entityType: "Course",
            entityId: notification.CourseId,
            actionUrl: $"/dashboard/courses/{notification.CourseId}",
            cancellationToken: ct);
    }
}