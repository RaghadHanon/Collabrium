using Domain.Enums;

namespace API.Contracts.Users;

public class UpdateUserRoleRequest
{
    public required UserRole NewRole { get; set; }
}