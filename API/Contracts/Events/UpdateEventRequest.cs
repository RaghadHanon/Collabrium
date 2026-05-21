namespace API.Contracts.Events;

public sealed class UpdateEventRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }

    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }

    public bool? IsOnline { get; set; }

    public string? Location { get; set; }
    public string? MeetingUrl { get; set; }

    public int? MaxAttendees { get; set; }
}