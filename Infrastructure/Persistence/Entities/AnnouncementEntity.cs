using Domain.Enums;

namespace Infrastructure.Persistence.Entities;
public sealed class AnnouncementEntity : DbEntity
{
    public string TenantId { get; set; } = null!;
    public TenantEntity Tenant { get; set; } = null!;

    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;

    public AnnouncementPriority Priority { get; set; }

    public DateTime PublishAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public string CreatedByUserId { get; set; } = null!;
    public ApplicationUserEntity CreatedByUser { get; set; } = null!;

    public ICollection<AnnouncementAudienceEntity> Audiences { get; set; }
        = new List<AnnouncementAudienceEntity>();
}