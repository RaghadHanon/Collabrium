using Application.Interfaces;
using Domain.Entities;

namespace Infrastructure.TenantContexts;
/// <summary>
/// Implementation of ITenantContext to manage tenant information in a multi-tenant application.
/// </summary>
public class TenantContext : ITenantContext
{
    /// <summary>
    /// Gets or sets the current tenant's identifier.
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the current tenant entity.
    /// </summary>
    public Tenant? CurrentTenant { get; set; }
}