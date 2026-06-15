using Domain.Models;

namespace Application.Interfaces.Repositories;
public interface IAnnouncementsRepository
{
    Task AddAsync(
        Announcement announcement,
        CancellationToken cancellationToken = default);

    Task<Announcement?> GetByIdAsync(
        string tenantId,
        string announcementId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string tenantId,
        string announcementId,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Announcement announcement,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        string tenantId,
        string announcementId,
        CancellationToken cancellationToken = default);
    Task<List<Announcement>> GetAnnouncementsDueForNotificationAsync(
      DateTime now,
      CancellationToken ct = default);
}