namespace API.Contracts.Events;

public sealed class GetEventsRequest
{
    public string? Search { get; set; }

    public bool? IsOnline { get; set; }
    public bool? HasAvailableSeats { get; set; }

    public DateTime? StartsAfter { get; set; }
    public DateTime? StartsBefore { get; set; }

    public int PageSize { get; set; } = 10;
    public DateTime? Cursor { get; set; }
}