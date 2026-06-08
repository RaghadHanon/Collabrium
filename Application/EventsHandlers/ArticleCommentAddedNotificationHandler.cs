using Application.Events;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;

namespace Application.EventsHandlers;
public sealed class ArticleCommentAddedNotificationHandler(
    INotificationCreationService notificationCreationService
) : INotificationHandler<ArticleCommentAddedEvent>
{
    public async Task Handle(ArticleCommentAddedEvent notification, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(notification.ArticleOwnerUserId))
            return;

        if (notification.ArticleOwnerUserId == notification.CommenterUserId)
            return;

        await notificationCreationService.CreateForUsersAsync(
            tenantId: notification.TenantId,
            recipientUserIds: [notification.ArticleOwnerUserId],
            type: NotificationType.ArticleCommentAdded,
            title: "New Article Comment",
            message: $"{notification.CommenterFullName} commented on your article {notification.ArticleTitle}.",
            triggeredByUserId: notification.CommenterUserId,
            entityType: "Article",
            entityId: notification.ArticleId,
            actionUrl: $"/learning-hub-management/articles/{notification.ArticleId}",
            cancellationToken: ct);
    }
}