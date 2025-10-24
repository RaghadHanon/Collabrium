using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Identity;

public class RoleSeeder
{
    public static async Task SeedRoles(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var roles = new[] {
            "SuperAdmin",
            "CEO",
            "HR",
            "ProductManager",
            "TeamLead",
            "DevOps",
            "BackendDeveloper",
            "FrontendDeveloper",
            "FullStackDeveloper",
            "QA",
            "BusinessAnalyst",
            "Designer"
        };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}