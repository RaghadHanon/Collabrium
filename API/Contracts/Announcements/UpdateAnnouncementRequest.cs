using Application.Features.Announcements.DTOs;
using Domain.Enums;

namespace API.Contracts.Announcements;

public sealed class UpdateAnnouncementRequest
{
    public string? Title { get; set; }

    public string? Content { get; set; }

    public AnnouncementPriority? Priority { get; set; }

    public DateTime? PublishAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool ClearExpiresAt { get; set; }

    public List<AnnouncementAudienceInputDto>? Audiences { get; set; }
}