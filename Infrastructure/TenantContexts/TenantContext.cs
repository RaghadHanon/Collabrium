using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.TenantContexts;
/// <summary>
/// Implementation of ITenantContext to manage tenant information in a multi-tenant application.
/// </summary>
public class TenantContext(IServiceProvider serviceProvider) : ITenantContext
{
    private const string GlobalTenantDomain = "global.platform";
    /// <summary>
    /// Gets or sets the current tenant's identifier.
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the current tenant entity.
    /// </summary>
    public Tenant? CurrentTenant { get; set; }

    /// <summary>
    /// Retrieves the global tenant from the database.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public async Task<Tenant?> GetGlobalTenant()
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await dbContext.Tenants
            .FirstOrDefaultAsync(t => t.Domain == GlobalTenantDomain);
    }
}