using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Announcements.Commands;
public sealed class DeleteAnnouncement
{
    public sealed class Command : IRequest<Result<Unit>>
    {
        public required string AnnouncementId { get; set; }
    }

    public sealed class Handler(
        IAnnouncementsRepository announcementsRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;

            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<Unit>.Failure(
                    "Tenant not found.",
                    HttpStatusCode.NotFound);

            var exists = await announcementsRepository.ExistsAsync(
                tenantId,
                request.AnnouncementId,
                cancellationToken);

            if (!exists)
                return Result<Unit>.Failure(
                    "Announcement not found.",
                    HttpStatusCode.NotFound);

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                await announcementsRepository.DeleteAsync(
                    tenantId,
                    request.AnnouncementId,
                    ct);

                await unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.OK);
        }
    }
}