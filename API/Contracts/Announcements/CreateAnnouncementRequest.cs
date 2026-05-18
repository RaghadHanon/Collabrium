using Application.Features.Announcements.DTOs;
using Domain.Enums;

namespace API.Contracts.Announcements;

public sealed class CreateAnnouncementRequest
{
    public required string Title { get; set; }

    public required string Content { get; set; }

    public AnnouncementPriority Priority { get; set; }
        = AnnouncementPriority.Normal;

    public DateTime PublishAt { get; set; }
        = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; }

    public List<AnnouncementAudienceInputDto> Audiences { get; set; }
        = new();
}