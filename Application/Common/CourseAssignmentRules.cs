using Domain.Enums;

namespace Application.Common;

public static class CourseAssignmentRules
{
    public static readonly UserRole[] BlockedTargetRoles =
    {
        UserRole.ProductOwner,
        UserRole.TeamLead,
        UserRole.TenantAdmin,
        UserRole.BusinessAnalyst,
        UserRole.CEO,
    };

    public static bool IsBlockedTargetRole(UserRole? role)
    {
        return role.HasValue && BlockedTargetRoles.Contains(role.Value);
    }

    public static bool IsBlockedJobTitle(string? jobTitle)
    {
        return string.IsNullOrWhiteSpace(jobTitle)
            ? false
            : BlockedTargetRoles
            .Select(r => r.ToString())
            .Contains(jobTitle);
    }

    public static string[] GetBlockedJobTitles()
    {
        return BlockedTargetRoles
            .Select(r => r.ToString())
            .ToArray();
    }
}
