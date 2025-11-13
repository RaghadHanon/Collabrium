using API.Middlewares;
using Domain.Enums;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure;
using System.Reflection;
using System.Text.Json.Serialization;

namespace API.SetupProgram;

/// <summary>
/// Service registration extensions for the API layer.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddControllers()
            .AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        // Validation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();

        // Swagger
        services.AddSwaggerWithJwtSupport();

        // App + Infrastructure
        services.AddInfrastructureServices(config);
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Global error handling
        services.AddExceptionHandler<GlobalExceptionHandler>();

        // Authorization policies
        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.SuperAdmin, p => p.RequireRole(nameof(UserRole.SuperAdmin)));
            options.AddPolicy(Policies.SuperAdmin, p => p.RequireRole(nameof(UserRole.GlopalAdmin)));
            options.AddPolicy(Policies.CEO, p => p.RequireRole(nameof(UserRole.CEO)));
            options.AddPolicy(Policies.HR, p => p.RequireRole(nameof(UserRole.HR)));
            options.AddPolicy(Policies.ProductManager, p => p.RequireRole(nameof(UserRole.ProductManager)));
            options.AddPolicy(Policies.TeamLead, p => p.RequireRole(nameof(UserRole.TeamLead)));
            options.AddPolicy(Policies.DevOps, p => p.RequireRole(nameof(UserRole.DevOps)));
            options.AddPolicy(Policies.BackendDeveloper, p => p.RequireRole(nameof(UserRole.BackendDev)));
            options.AddPolicy(Policies.FrontendDeveloper, p => p.RequireRole(nameof(UserRole.FrontendDev)));
            options.AddPolicy(Policies.FullStackDeveloper, p => p.RequireRole(nameof(UserRole.FullStackDev)));
            options.AddPolicy(Policies.QA, p => p.RequireRole(nameof(UserRole.QA)));
            options.AddPolicy(Policies.BusinessAnalyst, p => p.RequireRole(nameof(UserRole.BusinessAnalyst)));
            options.AddPolicy(Policies.Designer, p => p.RequireRole(nameof(UserRole.Designer)));
        });

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        services.AddSignalR();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}
