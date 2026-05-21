using Domain.Core;
using Domain.ValueObjects;

namespace Domain.Models;
public sealed class Event : Entity
{
    private readonly List<EventAttendance> _attendances = new();

    public string TenantId { get; private set; } = null!;

    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }

    public DateTime StartAt { get; private set; }
    public DateTime EndAt { get; private set; }

    public bool IsOnline { get; private set; }

    public string? Location { get; private set; }
    public string? MeetingUrl { get; private set; }

    public int? MaxAttendees { get; private set; }

    public StoredImage? BannerImage { get; private set; }

    public string CreatedByUserId { get; private set; } = null!;

    public bool IsCancelled { get; private set; }

    public IReadOnlyCollection<EventAttendance> Attendances => _attendances.AsReadOnly();
    private Event() { }

    public Event(
        string tenantId,
        string title,
        DateTime startAt,
        DateTime endAt,
        bool isOnline,
        string createdByUserId,
        string? description = null,
        string? location = null,
        string? meetingUrl = null,
        int? maxAttendees = null)
    {
        TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId)).Trim();
        Title = Guard.NotNullOrWhiteSpace(title, nameof(title)).Trim();
        CreatedByUserId = Guard.NotNullOrWhiteSpace(createdByUserId, nameof(createdByUserId)).Trim();

        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

        SetSchedule(startAt, endAt);
        SetEventPlace(isOnline, location, meetingUrl);
        SetMaxAttendees(maxAttendees);

        IsCancelled = false;

        Touch();
    }

    public Event(
        string id,
        string tenantId,
        string title,
        DateTime startAt,
        DateTime endAt,
        bool isOnline,
        string createdByUserId,
        string? description = null,
        string? location = null,
        string? meetingUrl = null,
        int? maxAttendees = null) : base(id)
    {
        TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId)).Trim();
        Title = Guard.NotNullOrWhiteSpace(title, nameof(title)).Trim();
        CreatedByUserId = Guard.NotNullOrWhiteSpace(createdByUserId, nameof(createdByUserId)).Trim();

        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

        SetSchedule(startAt, endAt);
        SetEventPlace(isOnline, location, meetingUrl);
        SetMaxAttendees(maxAttendees);

        IsCancelled = false;
    }
    public static Event Rehydrate(
        string id,
        DateTime createdAt,
        DateTime? updatedAt,
        string tenantId,
        string title,
        string? description,
        DateTime startAt,
        DateTime endAt,
        bool isOnline,
        string? location,
        string? meetingUrl,
        int? maxAttendees,
        StoredImage? bannerImage,
        string createdByUserId,
        bool isCancelled,
        IEnumerable<EventAttendance>? attendances)
    {
        var ev = new Event(
            id: id,
            tenantId: tenantId,
            title: title,
            startAt: startAt,
            endAt: endAt,
            isOnline: isOnline,
            createdByUserId: createdByUserId,
            description: description,
            location: location,
            meetingUrl: meetingUrl,
            maxAttendees: maxAttendees);

        ev.SetTimestamps(createdAt, updatedAt);
        ev.BannerImage = bannerImage;
        ev.IsCancelled = isCancelled;

        ev._attendances.Clear();

        if (attendances is not null)
        {
            foreach (var attendance in attendances)
                ev._attendances.Add(attendance);
        }

        return ev;
    }
    public void UpdateDetails(
        string title,
        string? description,
        DateTime startAt,
        DateTime endAt,
        bool isOnline,
        string? location,
        string? meetingUrl,
        int? maxAttendees)
    {
        Guard.Against(IsCancelled, "Cancelled event cannot be updated.");

        Title = Guard.NotNullOrWhiteSpace(title, nameof(title)).Trim();
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

        SetSchedule(startAt, endAt);
        SetEventPlace(isOnline, location, meetingUrl);
        SetMaxAttendees(maxAttendees);

        Touch();
    }
    public void Cancel()
    {
        if (IsCancelled)
            return;

        IsCancelled = true;
        Touch();
    }
    public void SetBannerImage(string url, string publicId)
    {
        BannerImage = StoredImage.Create(url, publicId);
        Touch();
    }
    public void RemoveBannerImage()
    {
        BannerImage = null;
        Touch();
    }

    public bool IsFinished(DateTime now)
    {
        return EndAt <= now;
    }
    public bool IsOngoing(DateTime now)
    {
        return StartAt <= now && EndAt > now;
    }

    public bool IsUpcoming(DateTime now)
    {
        return StartAt > now;
    }
    public string GetStatus(DateTime now)
    {
        if (IsCancelled)
            return "Cancelled";

        if (IsUpcoming(now))
            return "Upcoming";

        if (IsOngoing(now))
            return "Ongoing";

        return "Finished";
    }
    public int AttendeesCount()
    {
        return _attendances.Count;
    }

    public int? AvailableSeats()
    {
        if (MaxAttendees is null)
            return null;

        return Math.Max(MaxAttendees.Value - AttendeesCount(), 0);
    }
    private void SetSchedule(DateTime startAt, DateTime endAt)
    {
        Guard.Against(endAt <= startAt, "EndAt must be after StartAt.");

        StartAt = startAt;
        EndAt = endAt;
    }
    private void SetEventPlace(bool isOnline, string? location, string? meetingUrl)
    {
        IsOnline = isOnline;

        if (isOnline)
        {
            MeetingUrl = Guard.NotNullOrWhiteSpace(meetingUrl, nameof(meetingUrl)).Trim();
            Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim();
            return;
        }

        Location = Guard.NotNullOrWhiteSpace(location, nameof(location)).Trim();
        MeetingUrl = string.IsNullOrWhiteSpace(meetingUrl) ? null : meetingUrl.Trim();
    }

    private void SetMaxAttendees(int? maxAttendees)
    {
        if (maxAttendees.HasValue && maxAttendees.Value <= 0)
            throw new DomainException("MaxAttendees must be greater than 0.");

        MaxAttendees = maxAttendees;
    }


}
