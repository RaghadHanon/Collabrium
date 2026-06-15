using Application.Events;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.BackgroundServices;
public sealed class EventReminderBackgroundService(
    IServiceScopeFactory scopeFactory
) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan ReminderBeforeStart = TimeSpan.FromMinutes(15);

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

        var eventsRepository = scope.ServiceProvider.GetRequiredService<IEventsRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var now = DateTime.UtcNow;
        var remindBefore = now.Add(ReminderBeforeStart);

        var events = await eventsRepository.GetEventsDueForReminderAsync(
            now,
            remindBefore,
            ct);

        foreach (var ev in events)
        {
            var attendeeUserIds = await eventsRepository.GetAttendeeUserIdsAsync(
                ev.TenantId,
                ev.Id,
                ct);

            if (attendeeUserIds.Count > 0)
            {
                await mediator.Publish(new EventReminderDueEvent(
                    TenantId: ev.TenantId,
                    EventId: ev.Id,
                    EventTitle: ev.Title,
                    StartAt: ev.StartAt,
                    IsOnline: ev.IsOnline,
                    Location: ev.Location,
                    MeetingUrl: ev.MeetingUrl,
                    AttendeeUserIds: attendeeUserIds
                ), ct);
            }

            await unitOfWork.ExecuteInTransactionAsync(async tx =>
            {
                ev.MarkReminderSent(now);

                await eventsRepository.UpdateAsync(ev, tx);
            }, ct);
        }
    }
}