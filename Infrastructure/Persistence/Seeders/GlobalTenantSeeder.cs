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

                ContactDetails = new ContactDetailsOwned
                {
                    Email = "",
                    PhoneNumber = "",
                    Socials = new SocialLinksOwned
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

        var existingSuperAdmin = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == SuperAdminEmail && u.TenantId == globalTenant.Id);

        if (existingSuperAdmin is not null)
        {
            existingSuperAdmin.EmailConfirmed = true;

            if (string.IsNullOrWhiteSpace(globalTenant.TenantAdminId))
            {
                globalTenant.TenantAdminId = existingSuperAdmin.Id;
            }

            await dbContext.SaveChangesAsync();
            return;
        }

        var superAdminDomain = await identityService.RegisterUserAsync(
            fullName: "Super Admin",
            jobTitle: UserRole.SuperAdmin.ToString(),
            department: Department.Administration,
            password: SuperAdminPassword,
            dateOfBirth: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
            role: UserRole.SuperAdmin,
            tenantId: globalTenant.Id,
            email: SuperAdminEmail,
            phoneNumber: string.Empty
        );

        var createdSuperAdmin = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == superAdminDomain.Id);

        if (createdSuperAdmin is not null)
        {
            createdSuperAdmin.EmailConfirmed = true;
        }

        globalTenant.TenantAdminId = superAdminDomain.Id;
        await dbContext.SaveChangesAsync();
    }
}