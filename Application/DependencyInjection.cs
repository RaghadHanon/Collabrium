using Application.Common;
using Application.Common.Behaviors;
using Application.Features.Notifications.Services;
using Application.Interfaces;
using Application.Interfaces.Services;
using Application.Services;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddScoped<IProjectHistoryRecorder, ProjectHistoryRecorder>();

        services.AddScoped<INotificationCreationService, NotificationCreationService>();
        services.Configure<WebAppSettings>(configuration.GetSection("WebAppSettings"));

        return services;
    }
}