namespace API.Contracts.Events;

public sealed class GetCalendarEventsRequest
{
    public DateTime StartsAfter { get; set; }
    public DateTime StartsBefore { get; set; }
}