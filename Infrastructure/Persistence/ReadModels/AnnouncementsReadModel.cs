using Application.Common;
using Application.Features.Announcements.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Enums;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ReadModels;
public sealed class AnnouncementsReadModel : IAnnouncementsReadModel
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public AnnouncementsReadModel(AppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PagedList<AnnouncementDto, DateTime?>> GetAnnouncementsAsync(
        AnnouncementsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default)
    {
        parameters ??= new AnnouncementsSearchAndFilterParametersDto();

        var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

        IQueryable<AnnouncementEntity> query = _dbContext.Announcements
            .AsNoTracking();

        query = ApplyFilters(query, parameters);

        if (parameters.Cursor.HasValue)
            query = query.Where(a => a.CreatedAt < parameters.Cursor.Value);

        var items = await query
            .OrderByDescending(a => a.Priority)
            .ThenByDescending(a => a.PublishAt)
            .ThenByDescending(a => a.CreatedAt)
            .ProjectTo<AnnouncementDto>(_mapper.ConfigurationProvider)
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;

        if (items.Count > pageSize)
        {
            nextCursor = items[^1].CreatedAt;
            items.RemoveAt(items.Count - 1);
        }

        return new PagedList<AnnouncementDto, DateTime?>
        {
            Items = items,
            NextCursor = nextCursor
        };
    }

    public Task<AnnouncementDto?> GetByIdAsync(
        string tenantId,
        string announcementId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(announcementId))
            return Task.FromResult<AnnouncementDto?>(null);

        var tId = tenantId.Trim();
        var aId = announcementId.Trim();

        return _dbContext.Announcements
            .AsNoTracking()
            .Where(a => a.TenantId == tId && a.Id == aId)
            .ProjectTo<AnnouncementDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<AnnouncementDetailsDto?> GetDetailsByIdAsync(
        string tenantId,
        string announcementId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(announcementId))
            return Task.FromResult<AnnouncementDetailsDto?>(null);

        var tId = tenantId.Trim();
        var aId = announcementId.Trim();

        return _dbContext.Announcements
            .AsNoTracking()
            .Where(a => a.TenantId == tId && a.Id == aId)
            .ProjectTo<AnnouncementDetailsDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static IQueryable<AnnouncementEntity> ApplyFilters(
        IQueryable<AnnouncementEntity> query,
        AnnouncementsSearchAndFilterParametersDto p)
    {
        if (!string.IsNullOrWhiteSpace(p.TenantId))
        {
            var tenantId = p.TenantId.Trim();
            query = query.Where(a => a.TenantId == tenantId);
        }

        if (!string.IsNullOrWhiteSpace(p.Search))
        {
            var term = p.Search.Trim();

            query = query.Where(a =>
                a.Title.Contains(term) ||
                a.Content.Contains(term));
        }

        if (p.Priority.HasValue)
            query = query.Where(a => a.Priority == p.Priority.Value);

        if (p.OnlyVisibleNow == true)
        {
            var now = DateTime.UtcNow;

            query = query.Where(a =>
                a.PublishAt <= now &&
                (a.ExpiresAt == null || a.ExpiresAt > now));
        }

        if (p.PublishAfter.HasValue)
            query = query.Where(a => a.PublishAt >= p.PublishAfter.Value);

        if (p.PublishBefore.HasValue)
            query = query.Where(a => a.PublishAt <= p.PublishBefore.Value);

        if (p.ExpiresAfter.HasValue)
            query = query.Where(a =>
                a.ExpiresAt != null &&
                a.ExpiresAt >= p.ExpiresAfter.Value);

        if (p.ExpiresBefore.HasValue)
            query = query.Where(a =>
                a.ExpiresAt != null &&
                a.ExpiresAt <= p.ExpiresBefore.Value);

        if (p.CreatedAfter.HasValue)
            query = query.Where(a => a.CreatedAt >= p.CreatedAfter.Value);

        if (p.CreatedBefore.HasValue)
            query = query.Where(a => a.CreatedAt <= p.CreatedBefore.Value);

        if (!string.IsNullOrWhiteSpace(p.CurrentUserId))
        {
            var roleNames = p.CurrentUserRoleNames
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToList();

            var teamIds = p.CurrentUserTeamIds
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToList();

            query = p.Scope switch
            {
                AnnouncementScopeFilter.General =>
                    query.Where(a =>
                        a.Audiences.Any(x =>
                            x.Type == AnnouncementAudienceType.Everyone)),

                AnnouncementScopeFilter.MyTeams =>
                    query.Where(a =>
                        a.Audiences.Any(x =>
                            x.Type == AnnouncementAudienceType.Team &&
                            x.TeamId != null &&
                            teamIds.Contains(x.TeamId))),

                AnnouncementScopeFilter.MyRole =>
                    query.Where(a =>
                        a.Audiences.Any(x =>
                            x.Type == AnnouncementAudienceType.Role &&
                            x.RoleName != null &&
                            roleNames.Contains(x.RoleName))),

                _ =>
                    query.Where(a =>
                        a.Audiences.Any(x =>
                            x.Type == AnnouncementAudienceType.Everyone)

                        || a.Audiences.Any(x =>
                            x.Type == AnnouncementAudienceType.Role &&
                            x.RoleName != null &&
                            roleNames.Contains(x.RoleName))

                        || a.Audiences.Any(x =>
                            x.Type == AnnouncementAudienceType.Team &&
                            x.TeamId != null &&
                            teamIds.Contains(x.TeamId)))
            };
        }
        return query;
    }
}