namespace Application.Features.Tenants.DTOs;

public sealed class TenantWithAdminDto : TenantDto
{
    public TenantAdminDto? TenantAdmin { get; init; }
}