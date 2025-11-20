using Application.Features.Tenants.DTOs;
using Domain.Enums;

namespace Application.Features.Identity.DTOs;
public class UserDto
{
    public required string Id { get; set; }
    public required TenantDto Tenant { get; set; }
    public string FullName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public Department Department { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public DateOnly DateOfBirth { get; set; }

    public int Age { get; set; }
}