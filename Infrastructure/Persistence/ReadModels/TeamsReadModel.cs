using Application.Common;
using Application.Features.Teams.DTOs;
using Application.Features.Users.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ReadModels;

public sealed class TeamsReadModel : ITeamsReadModel
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public TeamsReadModel(AppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public Task<TeamDto?> GetByIdAsync(string tenantId, string teamId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(teamId))
            return Task.FromResult<TeamDto?>(null);

        tenantId = tenantId.Trim();
        teamId = teamId.Trim();

        return _dbContext.Teams
            .AsNoTracking()
            .Where(t => t.TenantId == tenantId && t.Id == teamId)
            .ProjectTo<TeamDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TeamFullInfoDto?> GetTeamFullAsync(string tenantId, string teamId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(teamId))
            return null;

        tenantId = tenantId.Trim();
        teamId = teamId.Trim();

        return await _dbContext.Teams
            .AsNoTracking()
            .Where(t => t.TenantId == tenantId && t.Id == teamId)
            .ProjectTo<TeamFullInfoDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<UserSnapshotDto>> GetTeamMembersSnapshotAsync(string tenantId, string teamId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(teamId))
            return new List<UserSnapshotDto>();

        tenantId = tenantId.Trim();
        teamId = teamId.Trim();

        return await _dbContext.TeamMembers
            .AsNoTracking()
            .Where(t => t.Team.TenantId == tenantId && t.TeamId == teamId)
            .ProjectTo<UserSnapshotDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UserDto>> GetCandidatesMembers(string tenantId, string teamId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(teamId))
            return new List<UserDto>();

        tenantId = tenantId.Trim();
        teamId = teamId.Trim();

        var teamMemberIds = await _dbContext.TeamMembers
            .AsNoTracking()
            .Where(tm => tm.Team.TenantId == tenantId && (tm.TeamId == null || tm.TeamId != teamId))
            .Select(tm => tm.UserId)
            .ToListAsync(cancellationToken);

        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.TenantId == tenantId && !teamMemberIds.Contains(u.Id))
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedList<TeamDto, DateTime?>> GetTeamsAsync(
        TeamsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default)
    {
        parameters ??= new TeamsSearchAndFilterParametersDto();
        var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

        var query = _dbContext.Teams.AsNoTracking();

        query = ApplyFilters(query, parameters);

        if (parameters.Cursor.HasValue)
            query = query.Where(t => t.CreatedAt >= parameters.Cursor.Value);

        var items = await query
            .OrderBy(t => t.CreatedAt)
            .ProjectTo<TeamDto>(_mapper.ConfigurationProvider)
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;
        if (items.Count > pageSize)
        {
            nextCursor = items.Last().CreatedAt;
            items.RemoveAt(items.Count - 1);
        }

        return new PagedList<TeamDto, DateTime?>
        {
            Items = items,
            NextCursor = nextCursor
        };
    }

    private static IQueryable<TeamEntity> ApplyFilters(IQueryable<TeamEntity> query, TeamsSearchAndFilterParametersDto p)
    {
        if (!string.IsNullOrWhiteSpace(p.TenantId))
            query = query.Where(t => t.TenantId == p.TenantId.Trim());

        if (!string.IsNullOrWhiteSpace(p.NameContains))
        {
            var term = p.NameContains.Trim();
            query = query.Where(t => t.Name.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(p.DescriptionContains))
        {
            var term = p.DescriptionContains.Trim();
            query = query.Where(t => t.Description != null && t.Description.Contains(term));
        }

        if (p.Department.HasValue)
            query = query.Where(t => t.Department == p.Department.Value);

        if (p.IsActive.HasValue)
            query = query.Where(t => t.IsActive == p.IsActive.Value);

        if (p.CapacityMin.HasValue)
            query = query.Where(t => t.Capacity >= p.CapacityMin.Value);

        if (p.CapacityMax.HasValue)
            query = query.Where(t => t.Capacity <= p.CapacityMax.Value);

        if (p.UpdatedAfter.HasValue)
            query = query.Where(t => t.UpdatedAt >= p.UpdatedAfter.Value);

        if (p.UpdatedBefore.HasValue)
            query = query.Where(t => t.UpdatedAt <= p.UpdatedBefore.Value);

        if (p.CreatedAfter.HasValue)
            query = query.Where(t => t.CreatedAt >= p.CreatedAfter.Value);

        if (p.CreatedBefore.HasValue)
            query = query.Where(t => t.CreatedAt <= p.CreatedBefore.Value);

        if (!string.IsNullOrWhiteSpace(p.TeamLeadName))
        {
            var term = p.TeamLeadName.Trim();
            query = query.Where(t => t.TeamLead != null && t.TeamLead.FullName.Contains(term));
        }

        return query;
    }
}
