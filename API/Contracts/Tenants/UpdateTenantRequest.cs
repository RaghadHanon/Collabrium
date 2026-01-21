namespace API.Contracts.Tenants;
/// <summary>
/// Request model for updating a tenant.
/// </summary>
public class UpdateTenantRequest
{
    public string? Name { get; set; }
    public string? Domain { get; set; }
    public string? Description { get; set; }
    public string? Country { get; set; }
    public IFormFile? File { get; set; }
    public string? WebsiteURL { get; set; }

    // Contact Info
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? LinkedIn { get; set; }
    public string? Facebook { get; set; }
    public string? Instagram { get; set; }
    public string? YouTube { get; set; }
}
