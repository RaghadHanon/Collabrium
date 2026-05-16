using Application.Common;
using Application.Features.Announcements.DTOs;

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