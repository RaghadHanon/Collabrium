using Application.Common.DTOs;

namespace Application.Features.Tenants.DTOs;
public class TenantContactInfoDto : ContactInfoDto
{
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
}
