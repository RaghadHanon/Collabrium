using Application.Features.Tenants.DTOs;
using Domain.Enums;

namespace Application.Features.Identity.DTOs;
public class UserWithTenantDto : UserDto
{
    public required TenantDto Tenant { get; set; }
}