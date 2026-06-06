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
public sealed class UpdateAnnouncement
{
    public sealed class Command : IRequest<Result<AnnouncementDetailsDto>>
    {
        public required string AnnouncementId { get; set; }

        public string? Title { get; set; }
        public string? Content { get; set; }

        public AnnouncementPriority? Priority { get; set; }

        public DateTime? PublishAt { get; set; }

        public DateTime? ExpiresAt { get; set; }
        public bool ClearExpiresAt { get; set; }

        public List<AnnouncementAudienceInputDto>? Audiences { get; set; }
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
            var currentUserId = currentUserAccessor.GetUserId();

            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<AnnouncementDetailsDto>.Failure(
                    "Tenant not found.",
                    HttpStatusCode.NotFound);

            var announcement = await announcementsRepository.GetByIdAsync(
                tenantId,
                request.AnnouncementId,
                cancellationToken);

            if (announcement is null)
                return Result<AnnouncementDetailsDto>.Failure(
                    "Announcement not found.",
                    HttpStatusCode.NotFound);
            if (!string.Equals(announcement.CreatedByUserId, currentUserId, StringComparison.Ordinal))
                return Result<AnnouncementDetailsDto>.Failure(
                    "You can update only your own announcement.",
                    HttpStatusCode.Forbidden);


            if (request.Audiences is not null && request.Audiences.Count == 0)
                return Result<AnnouncementDetailsDto>.Failure(
                    "Announcement must have at least one audience.",
                    HttpStatusCode.BadRequest);

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var finalTitle = PatchHelpers.ApplyStringPatch(
                    announcement.Title,
                    request.Title) ?? announcement.Title;

                var finalContent = PatchHelpers.ApplyStringPatch(
                    announcement.Content,
                    request.Content) ?? announcement.Content;

                var finalPriority = request.Priority ?? announcement.Priority;

                var finalPublishAt = request.PublishAt ?? announcement.PublishAt;

                DateTime? finalExpiresAt;

                if (request.ClearExpiresAt)
                    finalExpiresAt = null;
                else
                    finalExpiresAt = request.ExpiresAt ?? announcement.ExpiresAt;

                announcement.UpdateDetails(
                    title: finalTitle,
                    content: finalContent,
                    priority: finalPriority,
                    publishAt: finalPublishAt,
                    expiresAt: finalExpiresAt);

                if (request.Audiences is not null)
                {
                    var audiences = request.Audiences
                        .Select(a => CreateAudience(announcement.Id, a))
                        .ToList();

                    announcement.SetAudiences(audiences);
                }

                await announcementsRepository.UpdateAsync(announcement, ct);

                await unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);

            var dto = await announcementsReadModel.GetDetailsByIdAsync(
                tenantId,
                announcement.Id,
                cancellationToken);

            return dto is null
                ? Result<AnnouncementDetailsDto>.Failure(
                    "Announcement updated but could not be loaded.",
                    HttpStatusCode.InternalServerError)
                : Result<AnnouncementDetailsDto>.Success(dto, HttpStatusCode.OK);
        }

        private static AnnouncementAudience CreateAudience(
    string announcementId,
    AnnouncementAudienceInputDto input)
        {
            static string? Clean(string? value)
                => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

            return input.Type switch
            {
                AnnouncementAudienceType.Everyone =>
                    AnnouncementAudience.ForEveryone(announcementId),

                AnnouncementAudienceType.Team =>
                    Clean(input.TeamId) is { } teamId
                        ? AnnouncementAudience.ForTeam(announcementId, teamId)
                        : throw new DomainException("TeamId is required for team audience."),

                AnnouncementAudienceType.Role =>
                    input.RoleName.HasValue
                        ? AnnouncementAudience.ForRole(announcementId, input.RoleName.Value.ToString())
                        : throw new DomainException("RoleName is required for role audience."),

                _ => throw new DomainException("Invalid announcement audience type.")
            };
        }
    }
}