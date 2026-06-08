using Application.Events;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;

namespace Application.EventsHandlers;
public sealed class ArticleLikedNotificationHandler(
    INotificationCreationService notificationCreationService
) : INotificationHandler<ArticleLikedEvent>
{
    public async Task Handle(ArticleLikedEvent notification, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(notification.ArticleOwnerUserId))
            return;

        if (notification.ArticleOwnerUserId == notification.LikedByUserId)
            return;

        await notificationCreationService.CreateForUsersAsync(
            tenantId: notification.TenantId,
            recipientUserIds: [notification.ArticleOwnerUserId],
            type: NotificationType.ArticleLiked,
            title: "Article Liked",
            message: $"{notification.LikedByFullName} liked your article {notification.ArticleTitle}.",
            triggeredByUserId: notification.LikedByUserId,
            entityType: "Article",
            entityId: notification.ArticleId,
            actionUrl: $"/learning-hub-management/articles/{notification.ArticleId}",
            cancellationToken: ct);
    }
}