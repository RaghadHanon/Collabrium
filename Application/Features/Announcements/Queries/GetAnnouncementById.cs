using Application.Common;
using Application.Features.Announcements.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Announcements.Queries;
public sealed class GetAnnouncementById
{
    public sealed class Query : IRequest<Result<AnnouncementDto>>
    {
        public required string AnnouncementId { get; set; }
    }

    public sealed class Handler(
        IAnnouncementsRepository announcementsRepository,
        IAnnouncementsReadModel announcementsReadModel,
        ITenantContext tenantContext
    ) : IRequestHandler<Query, Result<AnnouncementDto>>
    {
        public async Task<Result<AnnouncementDto>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;

            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<AnnouncementDto>.Failure(
                    "Tenant not found.",
                    HttpStatusCode.NotFound);

            var exists = await announcementsRepository.ExistsAsync(
                tenantId,
                request.AnnouncementId,
                cancellationToken);

            if (!exists)
                return Result<AnnouncementDto>.Failure(
                    "Announcement not found.",
                    HttpStatusCode.NotFound);

            var dto = await announcementsReadModel.GetByIdAsync(
                tenantId,
                request.AnnouncementId,
                cancellationToken);

            return dto is null
                ? Result<AnnouncementDto>.Failure(
                    "Announcement not found.",
                    HttpStatusCode.NotFound)
                : Result<AnnouncementDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}