using Application.Common;
using Application.Features.Announcements.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Core;
using Domain.Enums;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.Announcements.Commands;
public sealed class CreateAnnouncement
{
    public sealed class Command : IRequest<Result<AnnouncementDetailsDto>>
    {
        public required string Title { get; set; }
        public required string Content { get; set; }

        public AnnouncementPriority Priority { get; set; } = AnnouncementPriority.Normal;

        public DateTime PublishAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }

        public List<AnnouncementAudienceInputDto> Audiences { get; set; } = new();
    }

    public sealed class Handler(
        IAnnouncementsRepository announcementsRepository,
        IAnnouncementsReadModel announcementsReadModel,
        ITenantContext tenantContext,
        ICurrentUserAccessor currentUserAccessor,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<Command, Result<AnnouncementDetailsDto>>
    {
        public async Task<Result<AnnouncementDetailsDto>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;

            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<AnnouncementDetailsDto>.Failure(
                    "Tenant not found.",
                    HttpStatusCode.NotFound);

            var currentUserId = currentUserAccessor.GetUserId();

            if (string.IsNullOrWhiteSpace(currentUserId))
                return Result<AnnouncementDetailsDto>.Failure(
                    "Current user not found.",
                    HttpStatusCode.Unauthorized);

            if (request.Audiences is null || request.Audiences.Count == 0)
                return Result<AnnouncementDetailsDto>.Failure(
                    "Announcement must have at least one audience.",
                    HttpStatusCode.BadRequest);

            Announcement announcement = null!;

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                announcement = new Announcement(
                    tenantId: tenantId,
                    title: request.Title,
                    content: request.Content,
                    priority: request.Priority,
                    publishAt: request.PublishAt,
                    expiresAt: request.ExpiresAt,
                    createdByUserId: currentUserId);

                var audiences = request.Audiences
                    .Select(a => CreateAudience(announcement.Id, a))
                    .ToList();

                announcement.SetAudiences(audiences);

                await announcementsRepository.AddAsync(announcement, ct);

                await unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);

            var dto = await announcementsReadModel.GetDetailsByIdAsync(
                tenantId,
                announcement.Id,
                cancellationToken);

            return dto is null
                ? Result<AnnouncementDetailsDto>.Failure(
                    "Announcement created but could not be loaded.",
                    HttpStatusCode.InternalServerError)
                : Result<AnnouncementDetailsDto>.Success(dto, HttpStatusCode.Created);
        }

        private static AnnouncementAudience CreateAudience(
            string announcementId,
            AnnouncementAudienceInputDto input)
        {
            return input.Type switch
            {
                AnnouncementAudienceType.Everyone =>
                    AnnouncementAudience.ForEveryone(announcementId),

                AnnouncementAudienceType.Team =>
                    AnnouncementAudience.ForTeam(announcementId, input.TeamId!),

                AnnouncementAudienceType.Role =>
                    AnnouncementAudience.ForRole(announcementId, input.RoleName!),

                _ => throw new DomainException("Invalid announcement audience type.")
            };
        }
    }
}