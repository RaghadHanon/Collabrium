using MediatR;

namespace Application.Events;
public sealed record AnnouncementCreatedEvent(
    string TenantId,
    string AnnouncementId
) : INotification;