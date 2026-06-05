namespace API.Contracts.Notifications;

public sealed class GetMyNotificationsRequest
{
    public DateTime? Cursor { get; init; }
    public int PageSize { get; init; } = 10;
    public bool? IsRead { get; init; }
}