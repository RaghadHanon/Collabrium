using Domain.Core;

namespace Domain.Models;
public sealed class EventAttendance : Entity
{
    public string TenantId { get; private set; } = null!;
    public string EventId { get; private set; } = null!;
    public string UserId { get; private set; } = null!;

    private EventAttendance() { }

    public EventAttendance(
        string tenantId,
        string eventId,
        string userId)
    {
        TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId)).Trim();
        EventId = Guard.NotNullOrWhiteSpace(eventId, nameof(eventId)).Trim();
        UserId = Guard.NotNullOrWhiteSpace(userId, nameof(userId)).Trim();

        Touch();
    }

    public EventAttendance(
        string id,
        string tenantId,
        string eventId,
        string userId) : base(id)
    {
        TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId)).Trim();
        EventId = Guard.NotNullOrWhiteSpace(eventId, nameof(eventId)).Trim();
        UserId = Guard.NotNullOrWhiteSpace(userId, nameof(userId)).Trim();
    }

    public static EventAttendance Rehydrate(
        string id,
        DateTime createdAt,
        DateTime? updatedAt,
        string tenantId,
        string eventId,
        string userId)
    {
        var attendance = new EventAttendance(
            id: id,
            tenantId: tenantId,
            eventId: eventId,
            userId: userId);

        attendance.SetTimestamps(createdAt, updatedAt);

        return attendance;
    }
}