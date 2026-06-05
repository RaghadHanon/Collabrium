using Domain.Enums;

namespace Application.Features.Announcements.DTOs;
public sealed class AnnouncementAudienceInputDto
{
    public AnnouncementAudienceType Type { get; set; }

    public string? TeamId { get; set; }

    public UserRole? RoleName { get; set; }

}