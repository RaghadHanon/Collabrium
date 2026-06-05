namespace Infrastructure.SignalR;

public static class NotificationGroups
{
    public static string Tenant(string tenantId)
        => $"tenant:{tenantId}";

    public static string Team(string tenantId, string teamId)
        => $"tenant:{tenantId}:team:{teamId}";

    public static string User(string tenantId, string userId)
        => $"tenant:{tenantId}:user:{userId}";
}