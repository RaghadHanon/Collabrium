namespace Domain.Models;

public sealed class TenantContactInfo : ContactInfo
{
    public string? Email { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; } = string.Empty;

    public void UpdateContact(string? email, string? phone)
    {
        Email = email ?? string.Empty;
        PhoneNumber = phone ?? string.Empty;
    }
}
