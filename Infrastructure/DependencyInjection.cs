using Application.Interfaces;
using Application.Interfaces.Emails;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Infrastructure.Email;
using Infrastructure.Email.Templates;
using Infrastructure.FilesStorage.Factory;
using Infrastructure.Identity;
using Infrastructure.Pdf;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.ReadModels;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Persistence.Seeders;
using Infrastructure.Security;
using Infrastructure.Settings;
using Infrastructure.SignalR;
using Infrastructure.Storage;
using Infrastructure.TenantContexts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using System.Text;
using System.Text.Json;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("SqlServer")));

        AddIdentity(services, configuration);

        AddRepositories(services);
        AddReadModels(services);
        AddAutoMapper(services);

        AddEmailServices(services, configuration);
        AddAuthorizationHandlers(services);

        AddStorageServices(services, configuration);

        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IChartService, ChartService>();
        QuestPDF.Settings.License = LicenseType.Community;

        services.AddSingleton<IUserPresenceTracker, InMemoryUserPresenceTracker>();

        return services;
    }

    private static void AddStorageServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));
        services.AddSingleton<ICloudinaryClientFactory, CloudinaryClientFactory>();

        services.AddScoped<CloudinaryImageStorage>();
        services.AddScoped<CloudinaryFileStorage>();
        services.AddScoped<CloudinaryVideoStorage>();

        services.AddScoped<IAttachmentStorageFactory, AttachmentStorageFactory>();
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<ITeamsRepository, TeamsRepository>();
        services.AddScoped<IProjectsRepository, ProjectsRepository>();
        services.AddScoped<IRoadmapsRepository, RoadmapsRepository>();
        services.AddScoped<ISprintsRepository, SprintsRepository>();
        services.AddScoped<IWorkItemsRepository, WorkItemsRepository>();
        services.AddScoped<ITagsRepository, TagsRepository>();
        services.AddScoped<IRoadmapNodesRepository, RoadmapNodesRepository>();
        services.AddScoped<IRoadmapSubTopicsRepository, RoadmapSubTopicsRepository>();
        services.AddScoped<IRoadmapResourcesRepository, RoadmapResourcesRepository>();
        services.AddScoped<IProjectHistoryRepository, ProjectHistoryRepository>();
    }

    private static void AddReadModels(IServiceCollection services)
    {
        services.AddScoped<IUsersReadModel, UsersReadModel>();
        services.AddScoped<ITenantsReadModel, TenantsReadModel>();
        services.AddScoped<ITeamsReadModel, TeamsReadModel>();
        services.AddScoped<IProjectsReadModel, ProjectsReadModel>();
        services.AddScoped<IRoadmapsReadModel, RoadmapsReadModel>();
        services.AddScoped<ISprintsReadModel, SprintsReadModel>();
        services.AddScoped<IWorkItemsReadModel, WorkItemsReadModel>();
        services.AddScoped<ITagsReadModel, TagsReadModel>();
        services.AddScoped<IRoadmapNodesReadModel, RoadmapNodesReadModel>();
        services.AddScoped<IRoadmapSubTopicsReadModel, RoadmapSubTopicsReadModel>();
        services.AddScoped<IRoadmapResourcesReadModel, RoadmapResourcesReadModel>();

        services.AddScoped<IProjectHistoriesReadModel, ProjectsHistoriesReadModel>();
        services.AddScoped<IReportsReadModel, ReportsReadModel>();
    }

    private static void AddAutoMapper(IServiceCollection services)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.AllowNullCollections = true;
            cfg.AllowNullDestinationValues = true;
        }, typeof(DependencyInjection).Assembly);
    }

    private static void AddEmailServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
        services.AddSingleton<IEmailService, SmtpEmailService>();
        services.AddScoped<IEmailTemplateFactory, EmailTemplateFactory>();

        var templateTypes = typeof(EmailTemplateFactory)
            .Assembly
            .GetTypes()
            .Where(t => typeof(IEmailTemplate).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in templateTypes)
            services.AddScoped(typeof(IEmailTemplate), type);
    }

    private static void AddAuthorizationHandlers(IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, IsPasswordChangedHandler>();
        services.AddScoped<IAuthorizationHandler, MustHaveProjectAccessHandler>();
    }

    private static void AddIdentity(IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<ApplicationUserEntity, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 1;

            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        var jwtSettingsSection = configuration.GetSection("JwtSettings");
        services.Configure<JwtSettings>(jwtSettingsSection);
        var jwtSettings = jwtSettingsSection.Get<JwtSettings>();

        services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
       .AddJwtBearer(options =>
       {
           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateIssuer = true,
               ValidateAudience = true,
               ValidateLifetime = true,
               ValidateIssuerSigningKey = true,
               ValidIssuer = jwtSettings?.Issuer,
               ValidAudience = jwtSettings?.Audience,
               IssuerSigningKey = new SymmetricSecurityKey(
                   Encoding.UTF8.GetBytes(jwtSettings?.SecretKey!)
               )
           };

           options.Events = new JwtBearerEvents
           {
               OnMessageReceived = context =>
               {
                   var accessToken = context.Request.Query["access_token"];
                   var path = context.HttpContext.Request.Path;

                   if (!string.IsNullOrEmpty(accessToken) &&
                       path.StartsWithSegments("/hubs"))
                   {
                       context.Token = accessToken;
                   }

                   return Task.CompletedTask;
               },

               OnChallenge = async context =>
               {
                   context.HandleResponse();

                   var problem = new ProblemDetails
                   {
                       Title = "Unauthorized",
                       Detail = "You must provide a valid access token.",
                       Status = StatusCodes.Status401Unauthorized
                   };

                   context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                   context.Response.ContentType = "application/json";
                   await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
               },

               OnForbidden = async context =>
               {
                   var problem = new ProblemDetails
                   {
                       Title = "Forbidden",
                       Detail = "You do not have permission to access this resource.",
                       Status = StatusCodes.Status403Forbidden
                   };

                   context.Response.StatusCode = StatusCodes.Status403Forbidden;
                   context.Response.ContentType = "application/json";
                   await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
               }
           };
       });

        services.AddScoped<RoleSeeder>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
        services.Configure<IdentitySettings>(configuration.GetSection("IdentitySettings"));
    }
}