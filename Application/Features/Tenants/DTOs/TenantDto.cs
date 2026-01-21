namespace Application.Features.Tenants.DTOs;
public class TenantDto
{
    public required string Id { get; set; }
    public string Name { get; set; } = null!;
    public string Domain { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public string? ProfileImageURL { get; set; }
    public string? Country { get; set; }
    public string? WebsiteURL { get; set; }
    public required TenantContactInfoDto TenantContactInfo { get; set; }
}