using API.Contracts.Identity;

namespace API.Contracts.Tenants;

/// <summary>
/// Represents a request to register a new tenant with an associated administrator.
/// </summary>
/// <remarks>This request includes the necessary information to create a tenant, such as the tenant's name and
/// domain. It extends the <see cref="RegisterAppUserRequest"/> to include additional tenant-specific details.</remarks>
public class RegisterTenantWithAdminRequest : RegisterAppUserRequest
{
    /// <summary>
    /// Gets or sets the name of the tenant.
    /// </summary>
    public required string TenantName { get; set; }
    /// <summary>
    /// Gets or sets the domain name associated with the current configuration.
    /// </summary>
    public required string Domain { get; set; }
    /// <summary>
    /// Gets or sets the description text.
    /// </summary>
    public string? Description { get; set; }
}
