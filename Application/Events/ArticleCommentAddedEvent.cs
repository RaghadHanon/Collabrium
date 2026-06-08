using MediatR;

namespace Application.Events;
public sealed record ArticleCommentAddedEvent(
    string TenantId,
    string ArticleId,
    string ArticleTitle,
    string ArticleOwnerUserId,
    string CommentId,
    string CommenterUserId,
    string CommenterFullName
) : INotification;