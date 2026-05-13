using Domain.Enums;

namespace Application.Common;

public static class RoadmapVisibilityRules
{
    private static readonly UserRole[] HiddenAccessRoles =
    {
         UserRole.ProductOwner,
        UserRole.TeamLead,
        UserRole.TenantAdmin,
        UserRole.BusinessAnalyst
    };

    public static bool CanViewHidden(UserRole? role)
    {
        return role.HasValue && HiddenAccessRoles.Contains(role.Value);
    }
}
