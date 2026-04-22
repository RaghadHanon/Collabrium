using API.Common.EnumHandlers;
using API.Middlewares;
using Application;
using Application.Common;
using Domain.Enums;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure;
using Infrastructure.Persistence.Serialization;
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
                opts.JsonSerializerOptions.Converters.Add(new SpacedEnumConverterFactory());
                opts.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
                opts.JsonSerializerOptions.Converters.Add(new DateTimeConverterUsingIso8601());
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
                policy.WithOrigins("http://localhost:5173",
                                   "http://localhost:5174",
                                   "http://localhost:5175",
                                   "http://localhost:5176", "http://localhost:5177")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        services.AddSignalR();
        services.AddProblemDetails();

        return services;
    }
}