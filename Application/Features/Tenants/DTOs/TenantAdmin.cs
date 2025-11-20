using Domain.Enums;

namespace Application.Features.Tenants.DTOs;
public class TenantAdmin
{
    public required string Id { get; set; }
    public string FullName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public Department Department { get; set; }
}
