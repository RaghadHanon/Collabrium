using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;
public static class EventAttendanceMapper
{
    public static EventAttendanceEntity ToEntity(EventAttendance domain)
    {
        ArgumentNullException.ThrowIfNull(domain);

        return new EventAttendanceEntity
        {
            Id = domain.Id,
            TenantId = domain.TenantId,
            EventId = domain.EventId,
            UserId = domain.UserId,
            CreatedAt = domain.CreatedAt,
            UpdatedAt = domain.UpdatedAt
        };
    }

    public static EventAttendance ToDomain(EventAttendanceEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return EventAttendance.Rehydrate(
            id: entity.Id,
            createdAt: entity.CreatedAt,
            updatedAt: entity.UpdatedAt,
            tenantId: entity.TenantId,
            eventId: entity.EventId,
            userId: entity.UserId);
    }
}