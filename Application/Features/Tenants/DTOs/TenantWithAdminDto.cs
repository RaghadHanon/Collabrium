namespace Application.Features.Tenants.DTOs;

public class TenantWithAdminDto : TenantDto
{
    public required TenantAdmin TenantAdmin { get; set; }
}
