using MediatR;

namespace Application.Events;
public sealed record EventReminderDueEvent(
    string TenantId,
    string EventId,
    string EventTitle,
    DateTime StartAt,
    bool IsOnline,
    string? Location,
    string? MeetingUrl,
    IReadOnlyList<string> AttendeeUserIds
) : INotification;