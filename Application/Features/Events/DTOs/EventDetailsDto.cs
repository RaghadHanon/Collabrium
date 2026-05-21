namespace Application.Features.Events.DTOs;
public sealed class EventDetailsDto : EventDto
{
    public IReadOnlyCollection<EventAttendeeDto> Attendees { get; set; }
        = Array.Empty<EventAttendeeDto>();
}