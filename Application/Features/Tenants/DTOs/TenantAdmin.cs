using Domain.Enums;

namespace Application.Features.Tenants.DTOs;

public sealed class TenantAdminDto
{
    public required string Id { get; init; }
    public required string FullName { get; init; }
    public string PhoneNumber { get; init; } = "";
    public string Email { get; init; } = "";
    public Department? Department { get; init; }
}