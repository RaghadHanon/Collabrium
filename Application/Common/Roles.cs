using Domain.Enums;

namespace Application.Common;
public static class Roles
{
    public static readonly List<UserRole?> ProjectsManagerRoles = [
            UserRole.TenantAdmin,
            UserRole.ProductOwner,
            UserRole.TeamLead,
            UserRole.BusinessAnalyst
        ];
}