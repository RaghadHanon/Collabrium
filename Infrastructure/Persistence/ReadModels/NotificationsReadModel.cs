using Application.Common;
using Application.Features.Notifications.DTOs;
using Application.Interfaces.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ReadModels;

public sealed class NotificationsReadModel(
    AppDbContext dbContext
) : INotificationsReadModel
{
    public async Task<PagedList<NotificationDto, DateTime?>> GetUserNotificationsAsync(
        string tenantId,
        string userId,
        NotificationsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default)
    {
        parameters ??= new NotificationsSearchAndFilterParametersDto();

        var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

        var tId = tenantId.Trim();
        var uId = userId.Trim();

        var query = dbContext.Notifications
            .AsNoTracking()
            .Where(n => n.TenantId == tId && n.RecipientUserId == uId);

        if (parameters.IsRead.HasValue)
            query = query.Where(n => n.IsRead == parameters.IsRead.Value);

        if (parameters.Cursor.HasValue)
            query = query.Where(n => n.CreatedAt < parameters.Cursor.Value);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(pageSize + 1)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                TenantId = n.TenantId,
                RecipientUserId = n.RecipientUserId,
                TriggeredByUserId = n.TriggeredByUserId,
                TriggeredByUserName = n.TriggeredByUser == null
                    ? null
                    : n.TriggeredByUser.FullName,
                Type = n.Type,
                Title = n.Title,
                Message = n.Message,
                EntityType = n.EntityType,
                EntityId = n.EntityId,
                ActionUrl = n.ActionUrl,
                IsRead = n.IsRead,
                ReadAt = n.ReadAt,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;

        if (items.Count > pageSize)
        {
            items.RemoveAt(items.Count - 1);
            nextCursor = items[^1].CreatedAt;
        }

        return new PagedList<NotificationDto, DateTime?>
        {
            Items = items,
            NextCursor = nextCursor
        };
    }

    public async Task<int> GetUnreadCountAsync(
        string tenantId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(userId))
            return 0;

        var tId = tenantId.Trim();
        var uId = userId.Trim();

        return await dbContext.Notifications
            .AsNoTracking()
            .CountAsync(
                n => n.TenantId == tId &&
                     n.RecipientUserId == uId &&
                     !n.IsRead,
                cancellationToken);
    }
}