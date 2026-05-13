using Application.Interfaces.Services;

namespace Application.Common;

public static class RoadmapAccessHelper
{
    public static async Task<bool> CanCurrentUserViewHiddenAsync(
        ICurrentUserAccessor currentUserAccessor,
        CancellationToken cancellationToken = default)
    {
        var userId = currentUserAccessor.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        var role = await currentUserAccessor.GetUserRole(userId, cancellationToken);

        return RoadmapVisibilityRules.CanViewHidden(role);
    }
}
