using Application.Events;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;

namespace Application.EventsHandlers;

public sealed class WorkItemCommentAddedNotificationHandler(
    INotificationCreationService notificationCreationService
) : INotificationHandler<WorkItemCommentAddedEvent>
{
    public async Task Handle(
        WorkItemCommentAddedEvent notification,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(notification.TenantId))
            return;

        if (notification.RecipientUserIds.Count == 0)
            return;

        var actionUrl = string.IsNullOrWhiteSpace(notification.SprintId)
            ? $"/projects/{notification.ProjectId}/backlog"
            : $"/projects/{notification.ProjectId}/board";

        await notificationCreationService.CreateForUsersAsync(
            tenantId: notification.TenantId,
            recipientUserIds: notification.RecipientUserIds,
            type: NotificationType.WorkItemCommentAdded,
            title: "New Work Item Comment",
            message: $"{notification.CommenterFullName} commented on {notification.WorkItemCode} - {notification.WorkItemTitle}.",
            triggeredByUserId: notification.CommenterUserId,
            entityType: "WorkItem",
            entityId: notification.WorkItemId,
            actionUrl: actionUrl,
            cancellationToken: ct);
    }
}