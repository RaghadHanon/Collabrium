using Application.Events;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;

namespace Application.EventsHandlers;

public sealed class WorkItemAssignedNotificationHandler(
    INotificationCreationService notificationCreationService
) : INotificationHandler<WorkItemAssignedEvent>
{
    public async Task Handle(WorkItemAssignedEvent notification, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(notification.AssigneeUserId))
            return;

        var actionUrl = string.IsNullOrWhiteSpace(notification.SprintId)
            ? $"/projects/{notification.ProjectId}/backlog/{notification.WorkItemId}"
            : $"/projects/{notification.ProjectId}/board/{notification.SprintId}/{notification.WorkItemId}";

        await notificationCreationService.CreateForUsersAsync(
            tenantId: notification.TenantId,
            recipientUserIds: new[] { notification.AssigneeUserId },
            type: NotificationType.WorkItemAssigned,
            title: "Work Item Assigned",
            message: $"{notification.AssignerFullName} assigned {notification.WorkItemCode} - {notification.WorkItemTitle} to you.",
            triggeredByUserId: notification.AssignerUserId,
            entityType: "WorkItem",
            entityId: notification.WorkItemId,
            actionUrl: actionUrl,
            cancellationToken: ct);
    }
}