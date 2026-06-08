using Application.Events;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;

namespace Application.EventsHandlers;
public sealed class PostLikedNotificationHandler(
    INotificationCreationService notificationCreationService
) : INotificationHandler<PostLikedEvent>
{
    public async Task Handle(PostLikedEvent notification, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(notification.PostOwnerUserId))
            return;

        if (notification.PostOwnerUserId == notification.LikedByUserId)
            return;

        await notificationCreationService.CreateForUsersAsync(
            tenantId: notification.TenantId,
            recipientUserIds: [notification.PostOwnerUserId],
            type: NotificationType.PostLiked,
            title: "Post Liked",
            message: $"{notification.LikedByFullName} liked your post {notification.PostTitle}.",
            triggeredByUserId: notification.LikedByUserId,
            entityType: "Post",
            entityId: notification.PostId,
            actionUrl: $"/community/posts/{notification.PostId}",
            cancellationToken: ct);
    }
}