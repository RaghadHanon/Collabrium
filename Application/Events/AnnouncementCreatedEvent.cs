using Application.Features.Announcements.DTOs;
using MediatR;

namespace Application.Events;
public sealed record AnnouncementCreatedEvent(
    string TenantId,
    string AnnouncementId,
    string Title,
    string CreatedByUserId,
    IReadOnlyList<AnnouncementAudienceDto> Audiences
) : INotification;