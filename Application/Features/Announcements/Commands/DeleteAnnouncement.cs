using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
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
        ICurrentUserAccessor currentUserAccessor,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            var currentUserId = currentUserAccessor.GetUserId();
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<Unit>.Failure(
                    "Tenant not found.",
                    HttpStatusCode.NotFound);

            if (string.IsNullOrWhiteSpace(currentUserId))
                return Result<Unit>.Failure(
                    "Current user not found.",
                    HttpStatusCode.Unauthorized);

            var announcement = await announcementsRepository.GetByIdAsync(
                tenantId,
                request.AnnouncementId,
                cancellationToken);

            if (announcement is null)
                return Result<Unit>.Failure(
                    "Announcement not found.",
                    HttpStatusCode.NotFound);

            if (!string.Equals(announcement.CreatedByUserId, currentUserId, StringComparison.Ordinal))
                return Result<Unit>.Failure(
                    "You can delete only your own announcement.",
                    HttpStatusCode.Forbidden);

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