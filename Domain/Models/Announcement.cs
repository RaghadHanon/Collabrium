using Domain.Core;
using Domain.Enums;

namespace Domain.Models;
public sealed class Announcement : Entity
{
    private readonly List<AnnouncementAudience> _audiences = new();

    public string TenantId { get; private set; } = null!;

    public string Title { get; private set; } = null!;
    public string Content { get; private set; } = null!;

    public AnnouncementPriority Priority { get; private set; }

    public DateTime PublishAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    public bool NotificationSent { get; private set; }
    public DateTime? NotificationSentAt { get; private set; }

    public string CreatedByUserId { get; private set; } = null!;

    public IReadOnlyCollection<AnnouncementAudience> Audiences => _audiences.AsReadOnly();

    private Announcement() { }

    public Announcement(
        string tenantId,
        string title,
        string content,
        AnnouncementPriority priority,
        DateTime publishAt,
        DateTime? expiresAt,
        string createdByUserId)
    {
        TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId)).Trim();
        Title = Guard.NotNullOrWhiteSpace(title, nameof(title)).Trim();
        Content = Guard.NotNullOrWhiteSpace(content, nameof(content)).Trim();
        Priority = priority;
        CreatedByUserId = Guard.NotNullOrWhiteSpace(createdByUserId, nameof(createdByUserId)).Trim();

        SetPublishAt(publishAt);
        SetExpiresAt(expiresAt);

        NotificationSent = false;
        NotificationSentAt = null;

        Touch();
    }

    public Announcement(
        string id,
        string tenantId,
        string title,
        string content,
        AnnouncementPriority priority,
        DateTime publishAt,
        DateTime? expiresAt,
        string createdByUserId) : base(id)
    {
        TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId)).Trim();
        Title = Guard.NotNullOrWhiteSpace(title, nameof(title)).Trim();
        Content = Guard.NotNullOrWhiteSpace(content, nameof(content)).Trim();
        Priority = priority;
        CreatedByUserId = Guard.NotNullOrWhiteSpace(createdByUserId, nameof(createdByUserId)).Trim();

        SetPublishAt(publishAt);
        SetExpiresAt(expiresAt);

        NotificationSent = false;
        NotificationSentAt = null;
    }

    public static Announcement Rehydrate(
        string id,
        DateTime createdAt,
        DateTime? updatedAt,
        string tenantId,
        string title,
        string content,
        AnnouncementPriority priority,
        DateTime publishAt,
        DateTime? expiresAt,
        string createdByUserId,
        bool notificationSent,
        DateTime? notificationSentAt,
        IEnumerable<AnnouncementAudience>? audiences)
    {
        var announcement = new Announcement(
            id,
            tenantId,
            title,
            content,
            priority,
            publishAt,
            expiresAt,
            createdByUserId);

        announcement.SetTimestamps(createdAt, updatedAt);
        announcement.NotificationSent = notificationSent;
        announcement.NotificationSentAt = notificationSentAt;

        announcement._audiences.Clear();

        if (audiences is not null)
        {
            foreach (var audience in audiences)
                announcement._audiences.Add(audience);
        }

        return announcement;
    }

    public void UpdateDetails(
        string title,
        string content,
        AnnouncementPriority priority,
        DateTime publishAt,
        DateTime? expiresAt)
    {
        Title = Guard.NotNullOrWhiteSpace(title, nameof(title)).Trim();
        Content = Guard.NotNullOrWhiteSpace(content, nameof(content)).Trim();
        Priority = priority;

        SetPublishAt(publishAt);
        SetExpiresAt(expiresAt);

        if (publishAt > DateTime.UtcNow)
        {
            NotificationSent = false;
            NotificationSentAt = null;
        }

        Touch();
    }

    public void SetAudiences(IEnumerable<AnnouncementAudience> audiences)
    {
        Guard.Against(audiences is null, "Audiences cannot be null.");

        _audiences.Clear();

        foreach (var audience in audiences)
        {
            if (audience.AnnouncementId != Id)
                audience.AttachToAnnouncement(Id);

            _audiences.Add(audience);
        }

        Guard.Against(_audiences.Count == 0, "Announcement must have at least one audience.");

        Touch();
    }

    public bool IsVisibleNow(DateTime now)
    {
        return PublishAt <= now &&
               (ExpiresAt is null || ExpiresAt > now);
    }

    public void MarkNotificationSent(DateTime sentAt)
    {
        if (NotificationSent)
            return;

        NotificationSent = true;
        NotificationSentAt = sentAt;
        Touch();
    }

    private void SetPublishAt(DateTime publishAt)
    {
        PublishAt = publishAt;
    }

    private void SetExpiresAt(DateTime? expiresAt)
    {
        if (expiresAt.HasValue && expiresAt.Value <= PublishAt)
            throw new DomainException("ExpiresAt must be after PublishAt.");

        ExpiresAt = expiresAt;
    }
}