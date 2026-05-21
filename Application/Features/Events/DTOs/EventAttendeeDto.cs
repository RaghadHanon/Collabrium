using Application.Features.Users.DTOs;

namespace Application.Features.Events.DTOs;
public sealed class EventAttendeeDto
{
    public required UserDto User { get; set; }

    public DateTime CreatedAt { get; set; }
}