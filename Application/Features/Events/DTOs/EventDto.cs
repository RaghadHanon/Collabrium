using Application.Features.Users.DTOs;

namespace Application.Features.Events.DTOs;
public class EventDto
{
    public required string Id { get; set; }

    public required string Title { get; set; }
    public string? Description { get; set; }

    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    public bool IsOnline { get; set; }

    public string? Location { get; set; }
    public string? MeetingUrl { get; set; }

    public int? MaxAttendees { get; set; }

    public string? BannerImageUrl { get; set; }
    public string? BannerImagePublicId { get; set; }

    public required UserDto CreatedByUser { get; set; }

    public bool IsCancelled { get; set; }
    public string Status { get; set; } = null!;

    public int AttendeesCount { get; set; }
    public int? AvailableSeats { get; set; }

    public bool IsCurrentUserInterested { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}