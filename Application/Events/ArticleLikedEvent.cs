using MediatR;

namespace Application.Events;
public sealed record ArticleLikedEvent(
    string TenantId,
    string ArticleId,
    string ArticleTitle,
    string ArticleOwnerUserId,
    string LikedByUserId,
    string LikedByFullName
) : INotification;