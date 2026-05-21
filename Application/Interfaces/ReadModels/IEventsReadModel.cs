using Application.Common;
using Application.Features.Events.DTOs;

namespace Application.Interfaces.ReadModels;
public interface IEventsReadModel
{
    Task<PagedList<EventDto, DateTime?>> GetEventsAsync(
        EventsSearchAndFilterParametersDto parameters,
        CancellationToken ct = default);

    Task<PagedList<EventDto, DateTime?>> GetManageEventsAsync(
        EventsSearchAndFilterParametersDto parameters,
        CancellationToken ct = default);

    Task<EventDto?> GetByIdAsync(
        string tenantId,
        string eventId,
        string? currentUserId = null,
        CancellationToken ct = default);

    Task<EventDetailsDto?> GetDetailsAsync(
        string tenantId,
        string eventId,
        string? currentUserId = null,
        CancellationToken ct = default);

    Task<List<EventCalendarItemDto>> GetCalendarEventsAsync(
        string tenantId,
        DateTime startsAfter,
        DateTime startsBefore,
        CancellationToken ct = default);
}