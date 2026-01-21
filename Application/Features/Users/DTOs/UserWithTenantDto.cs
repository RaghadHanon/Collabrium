using Application.Features.Tenants.DTOs;

namespace Application.Features.Users.DTOs;
public class UserWithTenantDto : UserDto
{
    public required TenantDto Tenant { get; set; }
}