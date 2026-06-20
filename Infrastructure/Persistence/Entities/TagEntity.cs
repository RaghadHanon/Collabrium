namespace Infrastructure.Persistence.Entities;

public class TagEntity : DbEntity
{
    public string TenantId { get; set; } = null!;
    public TenantEntity Tenant { get; set; } = null!;

    public string Name { get; set; } = null!;
}
