using API.Middlewares;
using Application;
using Application.Common;
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
                opts.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
            });

        // Validation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();

        // Swagger
        services.AddSwaggerWithJwtSupport();

        // App + Infrastructure
        services.AddInfrastructureServices(config);
        services.AddApplicationServices(config);
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Global error handling
        services.AddExceptionHandler<GlobalExceptionHandler>();

        // Authorization policies
        services.AddAppAuthorization();

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
