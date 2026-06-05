using Domain.Models;

namespace Application.Interfaces.Repositories;

public interface INotificationsRepository
{
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);

    Task AddRangeAsync(
        IEnumerable<Notification> notifications,
        CancellationToken cancellationToken = default);

    Task<Notification?> GetByIdAsync(
        string tenantId,
        string notificationId,
        CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(
        string tenantId,
        string notificationId,
        string recipientUserId,
        CancellationToken cancellationToken = default);

    Task MarkAllAsReadAsync(
        string tenantId,
        string recipientUserId,
        CancellationToken cancellationToken = default);
}