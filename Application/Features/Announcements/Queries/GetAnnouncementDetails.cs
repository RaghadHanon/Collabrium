using Application.Common;
using Application.Features.Announcements.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Announcements.Queries;
public sealed class GetAnnouncementDetails
{
    public sealed class Query : IRequest<Result<AnnouncementDetailsDto>>
    {
        public required string AnnouncementId { get; set; }
    }

    public sealed class Handler(
        IAnnouncementsRepository announcementsRepository,
        IAnnouncementsReadModel announcementsReadModel,
        ITenantContext tenantContext
    ) : IRequestHandler<Query, Result<AnnouncementDetailsDto>>
    {
        public async Task<Result<AnnouncementDetailsDto>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;

            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<AnnouncementDetailsDto>.Failure(
                    "Tenant not found.",
                    HttpStatusCode.NotFound);

            var exists = await announcementsRepository.ExistsAsync(
                tenantId,
                request.AnnouncementId,
                cancellationToken);

            if (!exists)
                return Result<AnnouncementDetailsDto>.Failure(
                    "Announcement not found.",
                    HttpStatusCode.NotFound);

            var dto = await announcementsReadModel.GetDetailsByIdAsync(
                tenantId,
                request.AnnouncementId,
                cancellationToken);

            return dto is null
                ? Result<AnnouncementDetailsDto>.Failure(
                    "Announcement not found.",
                    HttpStatusCode.NotFound)
                : Result<AnnouncementDetailsDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}