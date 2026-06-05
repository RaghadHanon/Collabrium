using Application.Events;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;

namespace Application.EventsHandlers;

public sealed class WorkItemUnassignedNotificationHandler(
    INotificationCreationService notificationCreationService
) : INotificationHandler<WorkItemUnassignedEvent>
{
    public async Task Handle(WorkItemUnassignedEvent notification, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(notification.PreviousAssigneeUserId))
            return;

        await notificationCreationService.CreateForUsersAsync(
            tenantId: notification.TenantId,
            recipientUserIds: new[] { notification.PreviousAssigneeUserId },
            type: NotificationType.WorkItemUnassigned,
            title: "Work Item Unassigned",
            message: $"{notification.AssignerFullName} unassigned you from {notification.WorkItemCode} - {notification.WorkItemTitle}.",
            triggeredByUserId: notification.AssignerUserId,
            entityType: "WorkItem",
            entityId: notification.WorkItemId,
            actionUrl: $"/projects/{notification.ProjectId}?tab=storyboard",
            cancellationToken: ct);
    }
}