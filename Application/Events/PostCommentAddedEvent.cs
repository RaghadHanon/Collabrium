using MediatR;

namespace Application.Events;
public sealed record PostCommentAddedEvent(
    string TenantId,
    string PostId,
    string PostTitle,
    string PostOwnerUserId,
    string CommentId,
    string CommenterUserId,
    string CommenterFullName
) : INotification;