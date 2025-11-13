using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence.Seeders;

public static class GlobalTenantSeeder
{
    private const string GlobalTenantName = "Platform Root Tenant";
    private const string GlobalTenantDomain = "global.platform";

    public static async Task SeedGlobalTenantAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Check if global tenant exists
        if (await dbContext.Tenants.AnyAsync(t => t.Domain == GlobalTenantDomain))
            return;

        var globalTenant = new Tenant
        {
            Id = "CollabriumGlobalTenant",
            Name = GlobalTenantName,
            Domain = GlobalTenantDomain,
            Description = "Global tenant for SuperAdmin and platform-level services.",
            IsActive = true
        };

        dbContext.Tenants.Add(globalTenant);
        await dbContext.SaveChangesAsync();
    }
}
