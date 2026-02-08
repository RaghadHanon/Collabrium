using API.Middlewares;
using Application;
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
        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.SuperAdmin, p => p.RequireRole(nameof(UserRole.SuperAdmin)));
            options.AddPolicy(Policies.TenantAdmin, p => p.RequireRole(nameof(UserRole.TenantAdmin)));
            options.AddPolicy(Policies.IT, p => p.RequireRole(nameof(UserRole.IT)));
            options.AddPolicy(Policies.CEO, p => p.RequireRole(nameof(UserRole.CEO)));
            options.AddPolicy(Policies.HR, p => p.RequireRole(nameof(UserRole.HR)));
            options.AddPolicy(Policies.ProductOwner, p => p.RequireRole(nameof(UserRole.ProductOwner)));
            options.AddPolicy(Policies.TeamLead, p => p.RequireRole(nameof(UserRole.TeamLead)));
            options.AddPolicy(Policies.BackendDev, p => p.RequireRole(nameof(UserRole.BackendDev)));
            options.AddPolicy(Policies.FrontendDev, p => p.RequireRole(nameof(UserRole.FrontendDev)));
            options.AddPolicy(Policies.FullStackDev, p => p.RequireRole(nameof(UserRole.FullStackDev)));
            options.AddPolicy(Policies.QA, p => p.RequireRole(nameof(UserRole.QA)));
            options.AddPolicy(Policies.BusinessAnalyst, p => p.RequireRole(nameof(UserRole.BusinessAnalyst)));
            options.AddPolicy(Policies.Designer, p => p.RequireRole(nameof(UserRole.Designer)));

            options.AddPolicy(Policies.TenantMember, p =>
                     p.RequireRole(
                         nameof(UserRole.TenantAdmin),
                         nameof(UserRole.IT),
                         nameof(UserRole.CEO),
                         nameof(UserRole.HR), 
                         nameof(UserRole.Designer),

                         nameof(UserRole.ProductOwner),
                         nameof(UserRole.TeamLead),
                         nameof(UserRole.BusinessAnalyst),

                         nameof(UserRole.BackendDev),
                         nameof(UserRole.FrontendDev), 
                         nameof(UserRole.FullStackDev),
                         nameof(UserRole.QA)
                     ));

            options.AddPolicy(Policies.ProductTeamManager, p =>
                     p.RequireRole(
                         nameof(UserRole.ProductOwner),
                         nameof(UserRole.TeamLead),
                         nameof(UserRole.BusinessAnalyst)
                     ));
            options.AddPolicy(Policies.ProductOwnerTeamLeadTenantAdmin, p =>
                     p.RequireRole(
                         nameof(UserRole.ProductOwner),
                         nameof(UserRole.TeamLead),
                         nameof(UserRole.TenantAdmin)
                     ));
            options.AddPolicy(Policies.ProductTeamMember, p =>
                    p.RequireRole(
                        nameof(UserRole.BackendDev),
                        nameof(UserRole.QA),
                        nameof(UserRole.FrontendDev)
                    ));
            options.AddPolicy(Policies.ProductTeamManagerTenantAdmin, p =>
                    p.RequireRole(
                        nameof(UserRole.ProductOwner),
                        nameof(UserRole.TeamLead),
                        nameof(UserRole.BusinessAnalyst),
                        nameof(UserRole.TenantAdmin)
                    ));

            options.AddPolicy(Policies.PasswordChanged, policy =>
                    policy.Requirements.Add(new IsPasswordChangedRequirement()));
            options.AddPolicy(Policies.MustHaveProjectAccess, policy =>
                    policy.Requirements.Add(new MustHaveProjectAccessRequirement()));
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
