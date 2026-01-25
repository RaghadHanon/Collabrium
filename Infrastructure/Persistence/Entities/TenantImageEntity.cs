namespace Infrastructure.Persistence.Entities;

public class TenantImageEntity : ImageEntity
{
    public string TenantId { get; set; } = null!;
    public TenantEntity Tenant { get; set; } = null!;
}
