using Application.Common.DTOs;

namespace Application.Features.Tenants.DTOs;

public class TenantDto
{
    public required string Id { get; init; }
    public DateTime CreatedAt { get; init; }

    public required string Name { get; init; }
    public required string Domain { get; init; }

    public string? Description { get; init; }
    public bool IsActive { get; init; }

    public string? ProfileImageUrl { get; init; }
    public string? Country { get; init; }
    public string? WebsiteUrl { get; init; }

    public ContactDetailsDto ContactInfo { get; init; } = new();
}