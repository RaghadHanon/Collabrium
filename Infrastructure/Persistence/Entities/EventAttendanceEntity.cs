namespace Infrastructure.Persistence.Entities;
public sealed class EventAttendanceEntity : DbEntity
{
    public string TenantId { get; set; } = null!;
    public TenantEntity Tenant { get; set; } = null!;

    public string EventId { get; set; } = null!;
    public EventEntity Event { get; set; } = null!;

    public string UserId { get; set; } = null!;
    public ApplicationUserEntity User { get; set; } = null!;
}