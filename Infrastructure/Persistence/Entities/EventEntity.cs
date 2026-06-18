using Domain.ValueObjects;

namespace Infrastructure.Persistence.Entities;
public sealed class EventEntity : DbEntity
{
    public string TenantId { get; set; } = null!;
    public TenantEntity Tenant { get; set; } = null!;

    public string Title { get; set; } = null!;
    public string? Description { get; set; }

    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    public bool IsOnline { get; set; }

    public string? Location { get; set; }
    public string? MeetingUrl { get; set; }

    public int? MaxAttendees { get; set; }

    public StoredImage? BannerImage { get; set; }

    public string CreatedByUserId { get; set; } = null!;
    public ApplicationUserEntity CreatedByUser { get; set; } = null!;

    public bool IsCancelled { get; set; }

    public bool ReminderSent { get; set; }
    public DateTime? ReminderSentAt { get; set; }

    public ICollection<EventAttendanceEntity> Attendances { get; set; }
        = new List<EventAttendanceEntity>();
}