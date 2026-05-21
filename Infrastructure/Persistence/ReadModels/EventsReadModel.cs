using Application.Common;
using Application.Features.Events.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ReadModels;
public sealed class EventsReadModel : IEventsReadModel
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public EventsReadModel(AppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PagedList<EventDto, DateTime?>> GetEventsAsync(
        EventsSearchAndFilterParametersDto parameters,
        CancellationToken ct = default)
    {
        parameters ??= new EventsSearchAndFilterParametersDto();

        var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;
        var currentUserId = parameters.CurrentUserId?.Trim();

        var query = _dbContext.Events.AsNoTracking();

        query = ApplyFilters(query, parameters);
        query = query.Where(e => !e.IsCancelled);

        if (parameters.Cursor.HasValue)
            query = query.Where(e => e.StartAt > parameters.Cursor.Value);

        var items = await query
            .OrderBy(e => e.StartAt)
            .ProjectTo<EventDto>(
                _mapper.ConfigurationProvider,
                new { currentUserId })
            .Take(pageSize + 1)
            .ToListAsync(ct);

        SetStatuses(items);

        DateTime? nextCursor = null;

        if (items.Count > pageSize)
        {
            nextCursor = items[^1].StartAt;
            items.RemoveAt(items.Count - 1);
        }

        return new PagedList<EventDto, DateTime?>
        {
            Items = items,
            NextCursor = nextCursor
        };
    }

    public async Task<PagedList<EventDto, DateTime?>> GetManageEventsAsync(
        EventsSearchAndFilterParametersDto parameters,
        CancellationToken ct = default)
    {
        parameters ??= new EventsSearchAndFilterParametersDto();

        var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;
        var currentUserId = parameters.CurrentUserId?.Trim();

        var query = _dbContext.Events.AsNoTracking();

        query = ApplyFilters(query, parameters);

        if (parameters.Cursor.HasValue)
            query = query.Where(e => e.CreatedAt < parameters.Cursor.Value);

        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .ProjectTo<EventDto>(
                _mapper.ConfigurationProvider,
                new { currentUserId })
            .Take(pageSize + 1)
            .ToListAsync(ct);

        SetStatuses(items);

        DateTime? nextCursor = null;

        if (items.Count > pageSize)
        {
            nextCursor = items[^1].CreatedAt;
            items.RemoveAt(items.Count - 1);
        }

        return new PagedList<EventDto, DateTime?>
        {
            Items = items,
            NextCursor = nextCursor
        };
    }

    public async Task<EventDto?> GetByIdAsync(
        string tenantId,
        string eventId,
        string? currentUserId = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(eventId))
            return null;

        var tId = tenantId.Trim();
        var eId = eventId.Trim();
        currentUserId = currentUserId?.Trim();

        var dto = await _dbContext.Events
            .AsNoTracking()
            .Where(e => e.TenantId == tId && e.Id == eId)
            .ProjectTo<EventDto>(
                _mapper.ConfigurationProvider,
                new { currentUserId })
            .FirstOrDefaultAsync(ct);

        if (dto is not null)
            dto.Status = GetStatus(dto.IsCancelled, dto.StartAt, dto.EndAt);

        return dto;
    }

    public async Task<EventDetailsDto?> GetDetailsAsync(
        string tenantId,
        string eventId,
        string? currentUserId = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(eventId))
            return null;

        var tId = tenantId.Trim();
        var eId = eventId.Trim();
        currentUserId = currentUserId?.Trim();

        var dto = await _dbContext.Events
            .AsNoTracking()
            .Where(e => e.TenantId == tId && e.Id == eId)
            .ProjectTo<EventDetailsDto>(
                _mapper.ConfigurationProvider,
                new { currentUserId })
            .FirstOrDefaultAsync(ct);

        if (dto is not null)
            dto.Status = GetStatus(dto.IsCancelled, dto.StartAt, dto.EndAt);

        return dto;
    }

    public async Task<List<EventCalendarItemDto>> GetCalendarEventsAsync(
        string tenantId,
        DateTime startsAfter,
        DateTime startsBefore,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            return new();

        var tId = tenantId.Trim();

        var items = await _dbContext.Events
            .AsNoTracking()
            .Where(e =>
                e.TenantId == tId &&
                e.StartAt >= startsAfter &&
                e.StartAt <= startsBefore &&
                !e.IsCancelled)
            .OrderBy(e => e.StartAt)
            .ProjectTo<EventCalendarItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        foreach (var item in items)
            item.Status = GetStatus(item.IsCancelled, item.StartAt, item.EndAt);

        return items;
    }

    private static void SetStatuses(IEnumerable<EventDto> items)
    {
        foreach (var item in items)
            item.Status = GetStatus(item.IsCancelled, item.StartAt, item.EndAt);
    }

    private static string GetStatus(bool isCancelled, DateTime startAt, DateTime endAt)
    {
        var now = DateTime.UtcNow;

        if (isCancelled)
            return "Cancelled";

        if (now < startAt)
            return "Upcoming";

        if (now >= startAt && now < endAt)
            return "Ongoing";

        return "Finished";
    }

    private static IQueryable<EventEntity> ApplyFilters(
        IQueryable<EventEntity> query,
        EventsSearchAndFilterParametersDto p)
    {
        if (!string.IsNullOrWhiteSpace(p.TenantId))
        {
            var tenantId = p.TenantId.Trim();
            query = query.Where(e => e.TenantId == tenantId);
        }

        if (!string.IsNullOrWhiteSpace(p.Search))
        {
            var term = p.Search.Trim();

            query = query.Where(e =>
                e.Title.Contains(term) ||
                (e.Description != null && e.Description.Contains(term)) ||
                (e.Location != null && e.Location.Contains(term)));
        }

        if (p.IsOnline.HasValue)
            query = query.Where(e => e.IsOnline == p.IsOnline.Value);

        if (p.IsCancelled.HasValue)
            query = query.Where(e => e.IsCancelled == p.IsCancelled.Value);

        if (p.StartsAfter.HasValue)
            query = query.Where(e => e.StartAt >= p.StartsAfter.Value);

        if (p.StartsBefore.HasValue)
            query = query.Where(e => e.StartAt <= p.StartsBefore.Value);

        if (p.EndsAfter.HasValue)
            query = query.Where(e => e.EndAt >= p.EndsAfter.Value);

        if (p.EndsBefore.HasValue)
            query = query.Where(e => e.EndAt <= p.EndsBefore.Value);

        if (p.CreatedAfter.HasValue)
            query = query.Where(e => e.CreatedAt >= p.CreatedAfter.Value);

        if (p.CreatedBefore.HasValue)
            query = query.Where(e => e.CreatedAt <= p.CreatedBefore.Value);

        if (p.HasAvailableSeats.HasValue)
        {
            query = p.HasAvailableSeats.Value
                ? query.Where(e =>
                    e.MaxAttendees == null ||
                    e.Attendances.Count < e.MaxAttendees.Value)
                : query.Where(e =>
                    e.MaxAttendees != null &&
                    e.Attendances.Count >= e.MaxAttendees.Value);
        }

        return query;
    }
}