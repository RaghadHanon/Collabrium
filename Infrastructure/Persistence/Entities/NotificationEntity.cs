using Domain.Enums;

namespace Infrastructure.Persistence.Entities;

public sealed class NotificationEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string TenantId { get; set; } = string.Empty;
    public TenantEntity Tenant { get; set; } = null!;

    public string RecipientUserId { get; set; } = string.Empty;
    public ApplicationUserEntity RecipientUser { get; set; } = null!;

    public string? TriggeredByUserId { get; set; }
    public ApplicationUserEntity? TriggeredByUser { get; set; }

    public NotificationType Type { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public string? EntityType { get; set; }
    public string? EntityId { get; set; }

    public string? ActionUrl { get; set; }

    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}