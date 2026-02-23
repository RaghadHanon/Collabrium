namespace Application.Interfaces;

public interface IUserPresenceTracker
{
    Task<bool> UserConnectedAsync(string tenantId, string userId, string connectionId);
    Task<bool> UserDisconnectedAsync(string tenantId, string userId, string connectionId);

    Task<IReadOnlyCollection<string>> GetOnlineUsersAsync(string tenantId);
    Task<bool> IsUserOnlineAsync(string tenantId, string userId);
}
