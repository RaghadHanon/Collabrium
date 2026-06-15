using Domain.Models;

namespace Application.Interfaces.Repositories;
public interface IEventsRepository
{
    Task AddAsync(Event ev, CancellationToken ct = default);

    Task UpdateAsync(Event ev, CancellationToken ct = default);

    Task<Event?> GetByIdAsync(
        string tenantId,
        string eventId,
        CancellationToken ct = default);

    Task<bool> ExistsAsync(
        string tenantId,
        string eventId,
        CancellationToken ct = default);

    Task<bool> RemoveAsync(
        string tenantId,
        string eventId,
        CancellationToken ct = default);

    Task<bool> HasAttendanceAsync(
        string tenantId,
        string eventId,
        string userId,
        CancellationToken ct = default);

    Task AddAttendanceAsync(
        EventAttendance attendance,
        CancellationToken ct = default);

    Task<bool> RemoveAttendanceAsync(
        string tenantId,
        string eventId,
        string userId,
        CancellationToken ct = default);

    Task<int> GetAttendeesCountAsync(
        string tenantId,
        string eventId,
        CancellationToken ct = default);

    Task<List<Event>> GetEventsDueForReminderAsync(
        DateTime now,
        DateTime remindBefore,
        CancellationToken ct = default);

    Task<List<string>> GetAttendeeUserIdsAsync(
        string tenantId,
        string eventId,
        CancellationToken ct = default);
}