using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;
public static class EventMapper
{
    public static EventEntity ToEntity(Event domain)
    {
        ArgumentNullException.ThrowIfNull(domain);

        return new EventEntity
        {
            Id = domain.Id,
            TenantId = domain.TenantId,
            Title = domain.Title,
            Description = domain.Description,
            StartAt = domain.StartAt,
            EndAt = domain.EndAt,
            IsOnline = domain.IsOnline,
            Location = domain.Location,
            MeetingUrl = domain.MeetingUrl,
            MaxAttendees = domain.MaxAttendees,
            BannerImage = domain.BannerImage,
            CreatedByUserId = domain.CreatedByUserId,
            IsCancelled = domain.IsCancelled,
            ReminderSent = domain.ReminderSent,
            ReminderSentAt = domain.ReminderSentAt,
            CreatedAt = domain.CreatedAt,
            UpdatedAt = domain.UpdatedAt
        };
    }

    public static Event ToDomain(EventEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var attendances = entity.Attendances?
            .Select(EventAttendanceMapper.ToDomain)
            .ToList();

        return Event.Rehydrate(
            id: entity.Id,
            createdAt: entity.CreatedAt,
            updatedAt: entity.UpdatedAt,
            tenantId: entity.TenantId,
            title: entity.Title,
            description: entity.Description,
            startAt: entity.StartAt,
            endAt: entity.EndAt,
            isOnline: entity.IsOnline,
            location: entity.Location,
            meetingUrl: entity.MeetingUrl,
            maxAttendees: entity.MaxAttendees,
            bannerImage: entity.BannerImage,
            createdByUserId: entity.CreatedByUserId,
            isCancelled: entity.IsCancelled,
            reminderSent: entity.ReminderSent,
            reminderSentAt: entity.ReminderSentAt,
            attendances: attendances
        );
    }

    public static void UpdateEntity(Event domain, EventEntity entity)
    {
        ArgumentNullException.ThrowIfNull(domain);
        ArgumentNullException.ThrowIfNull(entity);

        entity.Title = domain.Title;
        entity.Description = domain.Description;
        entity.StartAt = domain.StartAt;
        entity.EndAt = domain.EndAt;
        entity.IsOnline = domain.IsOnline;
        entity.Location = domain.Location;
        entity.MeetingUrl = domain.MeetingUrl;
        entity.MaxAttendees = domain.MaxAttendees;
        entity.BannerImage = domain.BannerImage;
        entity.CreatedByUserId = domain.CreatedByUserId;
        entity.IsCancelled = domain.IsCancelled;
        entity.ReminderSent = domain.ReminderSent;
        entity.ReminderSentAt = domain.ReminderSentAt;
        entity.UpdatedAt = domain.UpdatedAt ?? DateTime.UtcNow;
    }
}