namespace Infrastructure.Persistence.Entities;

public class TenantContactInfoEntity : ContactInfoEntity
{
    public string? Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; } = string.Empty;

    public string? TenantId { get; set; }
    public TenantEntity? Tenant { get; set; }
}
