namespace Domain.Entities;
public class TenantContactInfo : ContactInfo
{
    public string? Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; } = string.Empty;

    //Navigation property
    public string? TenantId { get; set; }
}
