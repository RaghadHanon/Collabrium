using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Infrastructure.SignalR.Hubs;

[Authorize]
public sealed class PresenceHub(
    ITenantContext tenantContext,
    IUserPresenceTracker tracker,
    ILogger<PresenceHub> logger
) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var tenantId = Context.User?.FindFirst("tenantId")?.Value
                       ?? tenantContext.TenantId;

        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? Context.User?.FindFirstValue("sub");

        logger.LogInformation("Claim tenantId = {TenantId}", Context.User?.FindFirst("tenantId")?.Value);
        logger.LogInformation("Scoped tenantContext.TenantId = {TenantId}", tenantContext.TenantId);

        logger.LogInformation(
            "PresenceHub connect. ConnectionId={ConnectionId}, TenantId={TenantId}, UserId={UserId}",
            Context.ConnectionId, tenantId, userId);

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            logger.LogWarning("PresenceHub aborted: tenantId missing. ConnectionId={ConnectionId}", Context.ConnectionId);
            Context.Abort();
            return;
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            logger.LogWarning("PresenceHub aborted: userId missing. ConnectionId={ConnectionId}", Context.ConnectionId);
            Context.Abort();
            return;
        }

        var tenantGroup = BuildTenantPresenceGroup(tenantId);
        await Groups.AddToGroupAsync(Context.ConnectionId, tenantGroup);

        var becameOnline = await tracker.UserConnectedAsync(tenantId, userId, Context.ConnectionId);
        var onlineUserIds = await tracker.GetOnlineUsersAsync(tenantId);

        await Clients.Caller.SendAsync("Presence:OnlineUsers", onlineUserIds);

        if (becameOnline)
            await Clients.Group(tenantGroup).SendAsync("Presence:UserOnline", new { UserId = userId });

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var tenantId = Context.User?.FindFirst("tenantId")?.Value
                       ?? tenantContext.TenantId;

        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? Context.User?.FindFirstValue("sub");

        logger.LogInformation(
            "PresenceHub disconnect. ConnectionId={ConnectionId}, TenantId={TenantId}, UserId={UserId}, Error={Error}",
            Context.ConnectionId, tenantId, userId, exception?.Message);

        if (!string.IsNullOrWhiteSpace(tenantId) && !string.IsNullOrWhiteSpace(userId))
        {
            var tenantGroup = BuildTenantPresenceGroup(tenantId);
            var becameOffline = await tracker.UserDisconnectedAsync(tenantId, userId, Context.ConnectionId);

            if (becameOffline)
                await Clients.Group(tenantGroup).SendAsync("Presence:UserOffline", new { UserId = userId });
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task<bool> IsOnline(string userId)
    {
        var tenantId = Context.User?.FindFirst("tenantId")?.Value
                       ?? tenantContext.TenantId;

        return string.IsNullOrWhiteSpace(tenantId) ? false : await tracker.IsUserOnlineAsync(tenantId, userId);
    }

    public async Task<IReadOnlyCollection<string>> GetOnlineUsers()
    {
        var tenantId = Context.User?.FindFirst("tenantId")?.Value
                       ?? tenantContext.TenantId;

        return string.IsNullOrWhiteSpace(tenantId) ? [] : await tracker.GetOnlineUsersAsync(tenantId);
    }

    private static string BuildTenantPresenceGroup(string tenantId)
    {
        return $"tenants:{tenantId}:presence";
    }
}