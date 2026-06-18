using MediatR;

namespace Application.Events;
public sealed record PostLikedEvent(
    string TenantId,
    string PostId,
    string PostTitle,
    string PostOwnerUserId,
    string LikedByUserId,
    string LikedByFullName
) : INotification;