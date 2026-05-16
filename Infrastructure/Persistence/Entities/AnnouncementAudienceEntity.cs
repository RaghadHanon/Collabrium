using Domain.Enums;

namespace Infrastructure.Persistence.Entities;
public sealed class AnnouncementAudienceEntity : DbEntity
{
    public string AnnouncementId { get; set; } = null!;
    public AnnouncementEntity Announcement { get; set; } = null!;

    public AnnouncementAudienceType Type { get; set; }

    public string? TeamId { get; set; }
    public TeamEntity? Team { get; set; }

    public string? RoleName { get; set; }

    public string? UserId { get; set; }
    public ApplicationUserEntity? User { get; set; }
}