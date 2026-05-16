using Application.Common;
using Application.Features.Announcements.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Services;

namespace Application.Features.Announcements.Queries;
using AutoMapper;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.Announcements.Queries;

public sealed class GetAnnouncements
{
    public sealed class Query : IRequest<Result<PagedList<AnnouncementDto, DateTime?>>>
    {
        public string? Search { get; set; }

        public AnnouncementPriority? Priority { get; set; }

        public AnnouncementScopeFilter Scope { get; set; } = AnnouncementScopeFilter.All;

        public DateTime? PublishAfter { get; set; }

        public DateTime? PublishBefore { get; set; }

        public DateTime? ExpiresAfter { get; set; }

        public DateTime? ExpiresBefore { get; set; }

        public DateTime? CreatedAfter { get; set; }

        public DateTime? CreatedBefore { get; set; }

        public int PageSize { get; set; } = 10;

        public DateTime? Cursor { get; set; }
    }

    public sealed class Handler(
        IAnnouncementsReadModel announcementsReadModel,
        ITeamsReadModel teamsReadModel,
        ITenantContext tenantContext,
        ICurrentUserAccessor currentUserAccessor,
        IMapper mapper
    ) : IRequestHandler<Query, Result<PagedList<AnnouncementDto, DateTime?>>>
    {
        public async Task<Result<PagedList<AnnouncementDto, DateTime?>>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;

            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<PagedList<AnnouncementDto, DateTime?>>.Failure(
                    "Tenant not found.",
                    HttpStatusCode.NotFound);

            var currentUserId = currentUserAccessor.GetUserId();

            if (string.IsNullOrWhiteSpace(currentUserId))
                return Result<PagedList<AnnouncementDto, DateTime?>>.Failure(
                    "Current user not found.",
                    HttpStatusCode.Unauthorized);

            var parameters = mapper.Map<AnnouncementsSearchAndFilterParametersDto>(request);

            parameters.TenantId = tenantId;
            parameters.CurrentUserId = currentUserId;
            parameters.OnlyVisibleNow = true;

            var role = await currentUserAccessor.GetUserRole(
                currentUserId,
                cancellationToken);

            if (role.HasValue)
            {
                parameters.CurrentUserRoleNames = new List<string>
                {
                    role.Value.ToString()
                };
            }

            parameters.CurrentUserTeamIds =
                await teamsReadModel.GetUserTeamIdsAsync(
                    tenantId,
                    currentUserId,
                    cancellationToken);

            var result = await announcementsReadModel.GetAnnouncementsAsync(
                parameters,
                cancellationToken);

            return Result<PagedList<AnnouncementDto, DateTime?>>.Success(
                result,
                HttpStatusCode.OK);
        }
    }
}