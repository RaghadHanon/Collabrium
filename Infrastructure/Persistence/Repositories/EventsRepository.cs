using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;
public sealed class EventsRepository : IEventsRepository
{
    private readonly AppDbContext _dbContext;

    public EventsRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddAsync(
        Event ev,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ev);

        var entity = EventMapper.ToEntity(ev);

        await _dbContext.Events.AddAsync(entity, ct);
    }

    public async Task UpdateAsync(
    Event ev,
    CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ev);

        var entity = await _dbContext.Events
            .FirstOrDefaultAsync(e =>
                e.Id == ev.Id &&
                e.TenantId == ev.TenantId,
                ct);

        if (entity is null)
            throw new InvalidOperationException("Event not found.");

        EventMapper.UpdateEntity(ev, entity);
    }

    public async Task<Event?> GetByIdAsync(
        string tenantId,
        string eventId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(eventId))
        {
            return null;
        }

        var tId = tenantId.Trim();
        var eId = eventId.Trim();

        var entity = await _dbContext.Events
            .AsNoTracking()
            .Include(e => e.Attendances)
            .FirstOrDefaultAsync(e =>
                e.TenantId == tId &&
                e.Id == eId,
                ct);

        return entity is null
            ? null
            : EventMapper.ToDomain(entity);
    }

    public Task<bool> ExistsAsync(
        string tenantId,
        string eventId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(eventId))
        {
            return Task.FromResult(false);
        }

        var tId = tenantId.Trim();
        var eId = eventId.Trim();

        return _dbContext.Events
            .AnyAsync(e =>
                e.TenantId == tId &&
                e.Id == eId,
                ct);
    }

    public async Task<bool> RemoveAsync(
        string tenantId,
        string eventId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(eventId))
        {
            return false;
        }

        var tId = tenantId.Trim();
        var eId = eventId.Trim();

        var affected = await _dbContext.Events
            .Where(e =>
                e.TenantId == tId &&
                e.Id == eId)
            .ExecuteDeleteAsync(ct);

        return affected > 0;
    }

    public Task<bool> HasAttendanceAsync(
        string tenantId,
        string eventId,
        string userId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(eventId) ||
            string.IsNullOrWhiteSpace(userId))
        {
            return Task.FromResult(false);
        }

        var tId = tenantId.Trim();
        var eId = eventId.Trim();
        var uId = userId.Trim();

        return _dbContext.EventAttendances
            .AnyAsync(a =>
                a.TenantId == tId &&
                a.EventId == eId &&
                a.UserId == uId,
                ct);
    }

    public async Task AddAttendanceAsync(
        EventAttendance attendance,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(attendance);

        var entity = EventAttendanceMapper.ToEntity(attendance);

        await _dbContext.EventAttendances
            .AddAsync(entity, ct);
    }

    public async Task<bool> RemoveAttendanceAsync(
        string tenantId,
        string eventId,
        string userId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(eventId) ||
            string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        var tId = tenantId.Trim();
        var eId = eventId.Trim();
        var uId = userId.Trim();

        var affected = await _dbContext.EventAttendances
            .Where(a =>
                a.TenantId == tId &&
                a.EventId == eId &&
                a.UserId == uId)
            .ExecuteDeleteAsync(ct);

        return affected > 0;
    }

    public Task<int> GetAttendeesCountAsync(
        string tenantId,
        string eventId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(eventId))
        {
            return Task.FromResult(0);
        }

        var tId = tenantId.Trim();
        var eId = eventId.Trim();

        return _dbContext.EventAttendances
            .CountAsync(a =>
                a.TenantId == tId &&
                a.EventId == eId,
                ct);
    }
}