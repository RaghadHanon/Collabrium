using Microsoft.OpenApi.Models;
using System.Reflection;

namespace API.SetupProgram;

/// <summary>
/// Swagger configuration extension.
/// </summary>
public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerWithJwtSupport(
        this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Collabrium API",
                Version = "v1",
                Description = "Collabrium Integrated Enterprise Management Platform API"
            });

            var xmlFilename =
                $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

            var xmlPath = Path.Combine(
                AppContext.BaseDirectory,
                xmlFilename);

            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            options.AddSecurityDefinition(
                "Bearer",
                new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description =
                        "Enter the JWT token. Example: Bearer eyJhbGci...",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

            options.AddSecurityRequirement(
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
        });

        return services;
    }
}