using Application.Common;
using Application.Features.Announcements.DTOs;
using Application.Features.Notifications.DTOs;

namespace Application.Interfaces.ReadModels;
public interface IAnnouncementsReadModel
{
    Task<PagedList<AnnouncementDto, DateTime?>> GetAnnouncementsAsync(
        AnnouncementsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default);

    Task<AnnouncementDto?> GetByIdAsync(
        string tenantId,
        string announcementId,
        CancellationToken cancellationToken = default);

    Task<AnnouncementDetailsDto?> GetDetailsByIdAsync(
        string tenantId,
        string announcementId,
        CancellationToken cancellationToken = default);
}

public interface INotificationsReadModel
{
    Task<PagedList<NotificationDto, DateTime?>> GetUserNotificationsAsync(
        string tenantId,
        string userId,
        NotificationsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(
        string tenantId,
        string userId,
        CancellationToken cancellationToken = default);
}