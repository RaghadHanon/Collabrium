using Application.Interfaces.Services;
using Domain.Enums;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence.Seeders;

public static class GlobalTenantSeeder
{
    private const string GlobalTenantId = "CollabriumGlobalTenant";
    private const string GlobalTenantName = "Platform Root Tenant";
    private const string GlobalTenantDomain = "global.platform";

    private const string SuperAdminEmail = "superadmin@platform.com";
    private const string SuperAdminPassword = "Pa$$w0rd";

    public static async Task SeedGlobalTenantAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();

        // 1) Ensure Global tenant exists
        var globalTenant = await dbContext.Tenants
            .FirstOrDefaultAsync(t => t.Domain == GlobalTenantDomain);

        if (globalTenant is null)
        {
            globalTenant = new TenantEntity
            {
                Id = GlobalTenantId,
                Name = GlobalTenantName,
                Domain = GlobalTenantDomain,
                Description = "Global tenant for SuperAdmin and platform-level services.",
                IsActive = true,
                WebsiteUrl = null,
                Country = null,

                // ✅ Owned ContactInfo (إذا عندك default empty، هذا اختياري)
                ContactInfo = new ContactDetailsOwned
                {
                    Email = "",
                    PhoneNumber = "",
                    Socials = new SocialLinksOwned()
                    {
                        LinkedIn = "",
                        Facebook = "",
                        Instagram = "",
                        YouTube = ""
                    }
                }
            };

            dbContext.Tenants.Add(globalTenant);
            await dbContext.SaveChangesAsync();
        }

        // 2) Ensure SuperAdmin exists for this tenant
        var existingSuperAdmin = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == SuperAdminEmail && u.TenantId == globalTenant.Id);

        if (existingSuperAdmin is not null)
        {
            // Ensure tenant has admin id set
            if (string.IsNullOrWhiteSpace(globalTenant.TenantAdminId))
            {
                globalTenant.TenantAdminId = existingSuperAdmin.Id;
                await dbContext.SaveChangesAsync();
            }

            return;
        }

        // 3) Create SuperAdmin user using IdentityService
        var superAdminDomain = await identityService.RegisterUserAsync(
            fullName: "Super Admin",
            department: Department.Administration,
            password: SuperAdminPassword,
            dateOfBirth: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
            role: UserRole.SuperAdmin,
            tenantId: globalTenant.Id,
            email: SuperAdminEmail,
            phoneNumber: string.Empty
        );

        // 4) Assign admin
        globalTenant.TenantAdminId = superAdminDomain.Id;
        await dbContext.SaveChangesAsync();
    }
}