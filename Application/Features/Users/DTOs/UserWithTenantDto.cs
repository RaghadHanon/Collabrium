using Application.Features.Tenants.DTOs;

namespace Application.Features.Users.DTOs;
public class UserWithTenantDto : UserDto
{
    public TenantDto? Tenant { get; set; }
}