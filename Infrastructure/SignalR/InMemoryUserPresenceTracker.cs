using Application.Interfaces;
using System.Collections.Concurrent;

namespace Infrastructure.SignalR;

public sealed class InMemoryUserPresenceTracker : IUserPresenceTracker
{
    /// <summary>
    /// <TenantId_1, <UserId_1, [ConnectionId_1]>,
    ///              <UserId_2, [ConnectionId_1, ConnectionId_2]>,
    ///              <UserId_3, [ConnectionId_3]>>
    /// ConcurrentDictionary is thread safe
    /// </summary>
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, HashSet<string>>> _store = new();

    public Task<bool> UserConnectedAsync(string tenantId, string userId, string connectionId)
    {
        var tenantUsers = _store.GetOrAdd(tenantId, _ => new ConcurrentDictionary<string, HashSet<string>>());
        var connections = tenantUsers.GetOrAdd(userId, _ => new HashSet<string>());

        // lock because HashSet not thread safe
        lock (connections)
        {
            var wasOffline = connections.Count == 0;
            connections.Add(connectionId);

            return Task.FromResult(wasOffline);
        }
    }

    public Task<bool> UserDisconnectedAsync(string tenantId, string userId, string connectionId)
    {
        if (!_store.TryGetValue(tenantId, out var tenantUsers))
            return Task.FromResult(false);

        if (!tenantUsers.TryGetValue(userId, out var connections))
            return Task.FromResult(false);

        lock (connections)
        {
            connections.Remove(connectionId);

            var isNowOffline = connections.Count == 0;
            if (isNowOffline)
                tenantUsers.TryRemove(userId, out _);

            return Task.FromResult(isNowOffline);
        }
    }

    public Task<IReadOnlyCollection<string>> GetOnlineUsersAsync(string tenantId)
    {
        return !_store.TryGetValue(tenantId, out var tenantUsers)
            ? Task.FromResult((IReadOnlyCollection<string>) Array.Empty<string>())
            : Task.FromResult((IReadOnlyCollection<string>) tenantUsers.Keys.ToList());
    }

    public Task<bool> IsUserOnlineAsync(string tenantId, string userId)
    {
        return !_store.TryGetValue(tenantId, out var tenantUsers)
            ? Task.FromResult(false)
            : Task.FromResult(tenantUsers.ContainsKey(userId));
    }
}