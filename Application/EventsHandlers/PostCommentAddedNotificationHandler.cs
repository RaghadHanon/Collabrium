using Application.Events;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;

namespace Application.EventsHandlers;
public sealed class PostCommentAddedNotificationHandler(
    INotificationCreationService notificationCreationService
) : INotificationHandler<PostCommentAddedEvent>
{
    public async Task Handle(PostCommentAddedEvent notification, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(notification.PostOwnerUserId))
            return;

        if (notification.PostOwnerUserId == notification.CommenterUserId)
            return;

        await notificationCreationService.CreateForUsersAsync(
            tenantId: notification.TenantId,
            recipientUserIds: [notification.PostOwnerUserId],
            type: NotificationType.PostCommentAdded,
            title: "New Post Comment",
            message: $"{notification.CommenterFullName} commented on your post {notification.PostTitle}.",
            triggeredByUserId: notification.CommenterUserId,
            entityType: "Post",
            entityId: notification.PostId,
            actionUrl: $"/community-hub/feed/{notification.PostId}",
            cancellationToken: ct);
    }
}