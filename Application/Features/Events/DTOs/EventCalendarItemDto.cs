namespace Application.Features.Events.DTOs;
public sealed class EventCalendarItemDto
{
    public required string Id { get; set; }
    public required string Title { get; set; }

    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    public bool IsOnline { get; set; }
    public bool IsCancelled { get; set; }

    public string Status { get; set; } = null!;
}