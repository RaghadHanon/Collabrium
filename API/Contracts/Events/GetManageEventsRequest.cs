namespace API.Contracts.Events;

public sealed class GetManageEventsRequest
{
    public string? Search { get; set; }

    public bool? IsOnline { get; set; }
    public bool? IsCancelled { get; set; }
    public bool? HasAvailableSeats { get; set; }

    public DateTime? StartsAfter { get; set; }
    public DateTime? StartsBefore { get; set; }

    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }

    public int PageSize { get; set; } = 10;
    public DateTime? Cursor { get; set; }
}