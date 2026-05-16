using Application.Features.Users.DTOs;
using Domain.Enums;

namespace Application.Features.Announcements.DTOs;
public sealed class AnnouncementAudienceDto
{
    public required string Id { get; set; }

    public AnnouncementAudienceType Type { get; set; }

    public string? TeamId { get; set; }
    public string? TeamName { get; set; }

    public string? RoleName { get; set; }

    public UserDto? User { get; set; }
}