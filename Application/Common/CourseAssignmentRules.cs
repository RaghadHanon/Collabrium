using Domain.Enums;

namespace Application.Common
{
    public static class CourseAssignmentRules
    {
        public static readonly UserRole[] BlockedTargetRoles =
        {
            UserRole.ProductOwner,
            UserRole.TeamLead,
            UserRole.TenantAdmin,
            UserRole.BusinessAnalyst
        };

        public static bool IsBlockedTargetRole(UserRole? role)
        {
            return role.HasValue && BlockedTargetRoles.Contains(role.Value);
        }
    }
}
