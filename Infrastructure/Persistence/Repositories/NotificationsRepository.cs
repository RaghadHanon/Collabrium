using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class NotificationsRepository(
    AppDbContext dbContext
) : INotificationsRepository
{
    public async Task AddAsync(
        Notification notification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var entity = NotificationMapper.ToEntity(notification);

        await dbContext.Notifications.AddAsync(entity, cancellationToken);
    }

    public async Task AddRangeAsync(
        IEnumerable<Notification> notifications,
        CancellationToken cancellationToken = default)
    {
        if (notifications is null)
            return;

        var entities = notifications
            .Select(NotificationMapper.ToEntity)
            .ToList();

        if (entities.Count == 0)
            return;

        await dbContext.Notifications.AddRangeAsync(entities, cancellationToken);
    }

    public async Task<Notification?> GetByIdAsync(
        string tenantId,
        string notificationId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(notificationId))
            return null;

        var tId = tenantId.Trim();
        var nId = notificationId.Trim();

        var entity = await dbContext.Notifications
            .AsNoTracking()
            .FirstOrDefaultAsync(
                n => n.TenantId == tId && n.Id == nId,
                cancellationToken);

        return entity is null ? null : NotificationMapper.ToDomain(entity);
    }

    public async Task MarkAsReadAsync(
        string tenantId,
        string notificationId,
        string recipientUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(notificationId) ||
            string.IsNullOrWhiteSpace(recipientUserId))
            return;

        var tId = tenantId.Trim();
        var nId = notificationId.Trim();
        var uId = recipientUserId.Trim();

        var entity = await dbContext.Notifications
            .FirstOrDefaultAsync(
                n => n.TenantId == tId &&
                     n.Id == nId &&
                     n.RecipientUserId == uId,
                cancellationToken);

        if (entity is null)
            return;

        if (entity.IsRead)
            return;

        entity.IsRead = true;
        entity.ReadAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
    }

    public async Task MarkAllAsReadAsync(
     string tenantId,
     string recipientUserId,
     CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(recipientUserId))
            return;

        var tId = tenantId.Trim();
        var uId = recipientUserId.Trim();
        var now = DateTime.UtcNow;

        await dbContext.Notifications
            .Where(n => n.TenantId == tId &&
                        n.RecipientUserId == uId &&
                        !n.IsRead)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, now)
                .SetProperty(n => n.UpdatedAt, now),
                cancellationToken);
    }
}