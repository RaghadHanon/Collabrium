using Application.Features.Notifications.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using Domain.Models;

namespace Application.Features.Notifications.Services;

public sealed class NotificationCreationService(
    INotificationsRepository notificationsRepository,
    IUnitOfWork unitOfWork
) : INotificationCreationService
{
    public async Task<IReadOnlyList<NotificationDto>> CreateForUsersAsync(
        string tenantId,
        IEnumerable<string> recipientUserIds,
        NotificationType type,
        string title,
        string message,
        string? triggeredByUserId = null,
        string? entityType = null,
        string? entityId = null,
        string? actionUrl = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            return [];

        if (recipientUserIds is null)
            return [];

        var userIds = recipientUserIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct()
            .ToList();

        if (userIds.Count == 0)
            return [];

        var notifications = userIds
            .Select(userId => new Notification(
                id: Guid.NewGuid().ToString(),
                tenantId: tenantId,
                recipientUserId: userId,
                type: type,
                title: title,
                message: message,
                triggeredByUserId: triggeredByUserId,
                entityType: entityType,
                entityId: entityId,
                actionUrl: actionUrl))
            .ToList();

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await notificationsRepository.AddRangeAsync(notifications, ct);
        }, cancellationToken);

        return notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            TenantId = n.TenantId,
            RecipientUserId = n.RecipientUserId,
            TriggeredByUserId = n.TriggeredByUserId,
            Type = n.Type,
            Title = n.Title,
            Message = n.Message,
            EntityType = n.EntityType,
            EntityId = n.EntityId,
            ActionUrl = n.ActionUrl,
            IsRead = n.IsRead,
            ReadAt = n.ReadAt,
            CreatedAt = n.CreatedAt
        }).ToList();
    }
}