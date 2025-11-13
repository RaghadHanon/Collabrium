using API.SetupProgram;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace API;

/// <summary>
/// The main entry point for the API application.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddApiServices(builder.Configuration);

        var app = builder.Build();

        await app.ApplyMigrationsAndSeedDataAsync();

        app.UseApiMiddleware();

        app.Run();
    }
}
