using Application.Features.Notifications.DTOs;
using Domain.Enums;

namespace Application.Interfaces.Services;

public interface INotificationCreationService
{
    Task<IReadOnlyList<NotificationDto>> CreateForUsersAsync(
        string tenantId,
        IEnumerable<string> recipientUserIds,
        NotificationType type,
        string title,
        string message,
        string? triggeredByUserId = null,
        string? entityType = null,
        string? entityId = null,
        string? actionUrl = null,
        CancellationToken cancellationToken = default);
}