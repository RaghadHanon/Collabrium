using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Infrastructure.SignalR.Hubs;

[Authorize]
public sealed class PresenceHub(
    ITenantContext tenantContext,
    IUserPresenceTracker tracker
) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var tenantId = tenantContext.TenantId;
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            Context.Abort();
            return;
        }

        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? Context.User?.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
        {
            Context.Abort();
            return;
        }

        var tenantGroup = BuildTenantPresenceGroup(tenantId);
        await Groups.AddToGroupAsync(Context.ConnectionId, tenantGroup);

        var becameOnline = await tracker.UserConnectedAsync(tenantId, userId, Context.ConnectionId);
        var onlineUserIds = await tracker.GetOnlineUsersAsync(tenantId);
        await Clients.Caller.SendAsync("Presence:OnlineUsers", onlineUserIds);

        // only if it's the first connection (tap) opened.
        if (becameOnline)
            await Clients.Group(tenantGroup).SendAsync("Presence:UserOnline", new { UserId = userId });

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var tenantId = tenantContext.TenantId;
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? Context.User?.FindFirstValue("sub");

        if (!string.IsNullOrWhiteSpace(tenantId) && !string.IsNullOrWhiteSpace(userId))
        {
            var tenantGroup = BuildTenantPresenceGroup(tenantId);

            var becameOffline = await tracker.UserDisconnectedAsync(tenantId, userId, Context.ConnectionId);

            // only if he closed all his connections (taps).
            if (becameOffline)
                await Clients.Group(tenantGroup).SendAsync("Presence:UserOffline", new { UserId = userId });
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task<bool> IsOnline(string userId)
    {
        var tenantId = tenantContext.TenantId;
        if (string.IsNullOrWhiteSpace(tenantId)) return false;

        return await tracker.IsUserOnlineAsync(tenantId, userId);
    }

    public async Task<IReadOnlyCollection<string>> GetOnlineUsers()
    {
        var tenantId = tenantContext.TenantId;

        if (string.IsNullOrWhiteSpace(tenantId))
            return [];

        return await tracker.GetOnlineUsersAsync(tenantId);
    }

    private static string BuildTenantPresenceGroup(string tenantId)
        => $"tenants:{tenantId}:presence";
}