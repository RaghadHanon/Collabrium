using Application.Features.Users.DTOs;
using Domain.Enums;

namespace Application.Features.Announcements.DTOs;
public sealed class AnnouncementDetailsDto
{
    public required string Id { get; set; }

    public required string Title { get; set; }

    public required string Content { get; set; }

    public AnnouncementPriority Priority { get; set; }

    public DateTime PublishAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool IsPublishedNow { get; set; }

    public bool IsExpired { get; set; }

    public required UserDto CreatedByUser { get; set; }

    public IReadOnlyList<AnnouncementAudienceDto> Audiences { get; set; }
        = new List<AnnouncementAudienceDto>();

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}