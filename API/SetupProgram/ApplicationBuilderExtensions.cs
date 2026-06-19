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
    public static IApplicationBuilder UseApiMiddleware(
        this WebApplication app)
    {
        app.UseExceptionHandler();

        var swaggerEnabled =
            app.Environment.IsDevelopment() ||
            app.Configuration.GetValue<bool>(
                "SwaggerSettings:Enabled");

        if (swaggerEnabled)
        {
            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint(
                    "/swagger/v1/swagger.json",
                    "Collabrium API v1");

                options.DocumentTitle =
                    "Collabrium API Documentation";

                options.RoutePrefix = "swagger";
            });
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseCors();

        app.UseAuthentication();

        app.UseMiddleware<TenantMiddleware>();

        app.UseAuthorization();

        app.MapHub<PresenceHub>("/hubs/presence");
        app.MapHub<CommentHub>("/hubs/comments");
        app.MapHub<NotificationHub>("/hubs/notifications");

        app.MapControllers();

        if (swaggerEnabled)
        {
            app.MapGet("/", () =>
                    Results.Redirect("/swagger"))
                .ExcludeFromDescription();
        }

        return app;
    }

    public static async Task ApplyMigrationsAndSeedDataAsync(
        this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        var applyMigrations = app.Configuration.GetValue<bool>(
            "DatabaseSettings:ApplyMigrationsOnStartup");

        var seedData = app.Configuration.GetValue<bool>(
            "DatabaseSettings:SeedDataOnStartup");

        if (!applyMigrations && !seedData)
        {
            logger.LogInformation(
                "Automatic database migrations and seeding are disabled.");

            return;
        }

        try
        {
            var dbContext =
                services.GetRequiredService<AppDbContext>();

            if (applyMigrations)
            {
                logger.LogInformation(
                    "Starting database migration...");

                await dbContext.Database.MigrateAsync();

                logger.LogInformation(
                    "Database migrated successfully.");
            }

            if (seedData)
            {
                logger.LogInformation(
                    "Starting database seeding...");

                await RoleSeeder.SeedRoles(services);

                await GlobalTenantSeeder
                    .SeedGlobalTenantAsync(services);

                logger.LogInformation(
                    "Database seeding completed.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "An error occurred during database migration/seeding.");

            throw;
        }
    }
}