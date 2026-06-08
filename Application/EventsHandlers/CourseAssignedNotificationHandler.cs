using Application.Events;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;

namespace Application.EventsHandlers;

public sealed class CourseAssignedNotificationHandler(
    INotificationCreationService notificationCreationService
) : INotificationHandler<CourseAssignedEvent>
{
    public async Task Handle(CourseAssignedEvent notification, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(notification.TenantId) ||
            string.IsNullOrWhiteSpace(notification.AssignedTo.Id))
            return;

        await notificationCreationService.CreateForUsersAsync(
            tenantId: notification.TenantId,
            recipientUserIds: [notification.AssignedTo.Id],
            type: NotificationType.CourseAssigned,
            title: "Course Assigned",
            message: $"{notification.AssignerFullName} assigned course {notification.CourseTitle} to you.",
            triggeredByUserId: notification.AssignerUserId,
            entityType: "Course",
            entityId: notification.CourseId,
            actionUrl: $"learning-hub/courses/{notification.CourseId}",
            cancellationToken: ct);
    }
}