using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Infrastructure.SignalR.Hubs;

[Authorize]
public sealed class NotificationHub(
    ITenantContext tenantContext,
    ITeamsReadModel teamsReadModel,
    ILogger<NotificationHub> logger
) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var tenantId = Context.User?.FindFirst("tenantId")?.Value
                       ?? tenantContext.TenantId
                       ?? tenantContext.CurrentTenant?.Id;

        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? Context.User?.FindFirstValue("sub");

        logger.LogInformation(
            "NotificationHub connected. ConnectionId={ConnectionId}, TenantId={TenantId}, UserId={UserId}",
            Context.ConnectionId,
            tenantId,
            userId);

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            logger.LogWarning("NotificationHub connection aborted: tenantId is missing.");
            Context.Abort();
            return;
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            logger.LogWarning("NotificationHub connection aborted: userId is missing.");
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            NotificationGroups.Tenant(tenantId));

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            NotificationGroups.User(tenantId, userId));

        var teamIds = await teamsReadModel.GetUserTeamIdsAsync(
            tenantId,
            userId,
            Context.ConnectionAborted);

        foreach (var teamId in teamIds.Distinct())
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                NotificationGroups.Team(tenantId, teamId));
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation(
            "NotificationHub disconnected. ConnectionId={ConnectionId}",
            Context.ConnectionId);

        await base.OnDisconnectedAsync(exception);
    }
}