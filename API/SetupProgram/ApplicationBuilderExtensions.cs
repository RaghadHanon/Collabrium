using Infrastructure.Persistence;
using Infrastructure.Persistence.Seeders;
using Infrastructure.SignalR.Hubs;
using Microsoft.EntityFrameworkCore;

namespace API.SetupProgram;

/// <summary>
/// Application builder extensions for configuring middleware and migrations.
/// </summary>
public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseApiMiddleware(this WebApplication app)
    {
        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection(); 
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<TenantMiddleware>();
        app.MapHub<PresenceHub>("/hubs/presence");
        app.MapHub<CommentHub>("/hubs/comments");
        app.MapControllers();

        return app;
    }

    public static async Task ApplyMigrationsAndSeedDataAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Starting database migration...");

            var dbContext = services.GetRequiredService<AppDbContext>();
            await dbContext.Database.MigrateAsync();

            logger.LogInformation("Database migrated successfully.");
            logger.LogInformation("Seeding...");

            await RoleSeeder.SeedRoles(services);
            await GlobalTenantSeeder.SeedGlobalTenantAsync(services);

            logger.LogInformation("Database seeding completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during database migration/seeding.");
            throw; // stop app if DB migration/seeding fails
        }
    }
}
