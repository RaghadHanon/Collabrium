namespace Domain.Entities;
public class TenantImage : Image
{
    public required string TenantId { get; set; }
}
