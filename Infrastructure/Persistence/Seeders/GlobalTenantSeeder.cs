using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence.Seeders;

public static class GlobalTenantSeeder
{
    private const string GlobalTenantName = "Platform Root Tenant";
    private const string GlobalTenantDomain = "global.platform";
    private const string SuperAdminEmail = "superadmin@platform.com";
    private const string SuperAdminPassword = "Pa$$w0rd";

    public static async Task SeedGlobalTenantAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();

        var globalTenant = await dbContext.Tenants
            .Include(t => t.Users)
            .FirstOrDefaultAsync(t => t.Domain == GlobalTenantDomain);

        if (globalTenant == null)
        {
            globalTenant = new Tenant
            {
                Id = "CollabriumGlobalTenant",
                Name = GlobalTenantName,
                Domain = GlobalTenantDomain,
                Description = "Global tenant for SuperAdmin and platform-level services.",
                IsActive = true,
                TenantContactInfo = new TenantContactInfo
                {
                    TenantId = "CollabriumGlobalTenant",
                    Email = string.Empty,
                    PhoneNumber = string.Empty,
                    LinkedIn = string.Empty,
                    Facebook = string.Empty,
                    Instagram = string.Empty,
                    YouTube = string.Empty
                }
            };

            dbContext.Tenants.Add(globalTenant);
            await dbContext.SaveChangesAsync();
        }

        var existingSuperAdmin = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == SuperAdminEmail && u.TenantId == globalTenant.Id);

        if (existingSuperAdmin != null)
            return;

        var superAdmin = await identityService.RegisterUserAsync(
            fullName: "Super Admin",
            department: Department.Administration,
            password: SuperAdminPassword,
            dateOfBirth: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
            role: UserRole.SuperAdmin,
            tenantId: globalTenant.Id,
            email: SuperAdminEmail,
            phoneNumber: string.Empty
        );

        globalTenant.TenantAdminId = superAdmin.Id;
        globalTenant.Users.Add(superAdmin);
        await dbContext.SaveChangesAsync();
    }
}
