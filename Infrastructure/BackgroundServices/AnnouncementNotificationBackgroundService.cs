using Application.Events;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.BackgroundServices;
public sealed class AnnouncementNotificationBackgroundService(
    IServiceScopeFactory scopeFactory
) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(CheckInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessAsync(stoppingToken);
        }
    }

    private async Task ProcessAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();

        var announcementsRepository = scope.ServiceProvider.GetRequiredService<IAnnouncementsRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var now = DateTime.UtcNow;

        var announcements = await announcementsRepository.GetAnnouncementsDueForNotificationAsync(
            now,
            ct);

        foreach (var announcement in announcements)
        {
            await mediator.Publish(new AnnouncementCreatedEvent(
                TenantId: announcement.TenantId,
                AnnouncementId: announcement.Id
            ), ct);

            await unitOfWork.ExecuteInTransactionAsync(async tx =>
            {
                announcement.MarkNotificationSent(now);
                await announcementsRepository.UpdateAsync(announcement, tx);
            }, ct);
        }
    }
}