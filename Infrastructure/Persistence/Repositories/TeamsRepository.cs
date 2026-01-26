using Application.Common;
using Application.Features.Teams.DTOs;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class TeamsRepository : ITeamsRepository
{
    private readonly AppDbContext _dbContext;

    public TeamsRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    // -----------------------
    // Write
    // -----------------------

    public async Task AddAsync(Team team, CancellationToken cancellationToken = default)
    {
        if (team is null) throw new ArgumentNullException(nameof(team));

        var entity = TeamMapper.ToEntity(team);

        // ✅ add leader as member by default عبر join table
        // lead لازم يكون member (domain invariant)، لكن DB كمان لازم يعكسه
        entity.Members.Add(new TeamMemberEntity
        {
            TeamId = entity.Id,
            UserId = team.TeamLeadId,
            JoinedAt = DateTime.UtcNow
        });

        await _dbContext.Teams.AddAsync(entity, cancellationToken);
    }

    public async Task<bool> RemoveAsync(string tenantId, string teamId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(teamId))
            return false;

        var entity = await _dbContext.Teams
            .FirstOrDefaultAsync(t => t.TenantId == tenantId.Trim() && t.Id == teamId.Trim(), cancellationToken);

        if (entity is null) return false;

        _dbContext.Teams.Remove(entity);
        return true;
    }

    public async Task UpdateAsync(Team team, CancellationToken cancellationToken = default)
    {
        if (team is null) throw new ArgumentNullException(nameof(team));

        var entity = await _dbContext.Teams
            .FirstOrDefaultAsync(t => t.Id == team.Id, cancellationToken);

        if (entity is null)
            throw new InvalidOperationException($"Team '{team.Id}' was not found.");

        TeamMapper.UpdateEntity(team, entity);
    }

    public async Task ToggleActivationAsync(string tenantId, string teamId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(teamId))
            return;

        var entity = await _dbContext.Teams
            .FirstOrDefaultAsync(t => t.TenantId == tenantId.Trim() && t.Id == teamId.Trim(), cancellationToken);

        if (entity is null) return;

        entity.IsActive = !entity.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
    }

    public async Task AddMembersAsync(
        string tenantId,
        string teamId,
        IReadOnlyCollection<string> memberIds,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(teamId)) throw new ArgumentNullException(nameof(teamId));
        if (memberIds is null || memberIds.Count == 0) return;

        tenantId = tenantId.Trim();
        teamId = teamId.Trim();

        var team = await _dbContext.Teams
            .Include(t => t.Members) // join rows
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == teamId, cancellationToken);

        if (team is null) throw new InvalidOperationException("Team not found.");
        if (!team.IsActive) throw new InvalidOperationException("Cannot add members to an inactive team.");

        var ids = memberIds
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (ids.Count == 0) return;

        // تأكد المستخدمين موجودين وبنفس tenant
        var users = await _dbContext.Users
            .AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, u.TenantId })
            .ToListAsync(cancellationToken);

        if (users.Count != ids.Count)
            throw new InvalidOperationException("One or more users were not found.");

        if (users.Any(u => u.TenantId != tenantId))
            throw new InvalidOperationException("One or more users are not in the current tenant.");

        var existingIds = team.Members.Select(m => m.UserId).ToHashSet(StringComparer.Ordinal);
        if (ids.Any(existingIds.Contains))
            throw new InvalidOperationException("One or more users are already members of the team.");

        if (team.Capacity.HasValue && existingIds.Count + ids.Count > team.Capacity.Value)
            throw new InvalidOperationException("Adding these members exceeds the team capacity.");

        foreach (var userId in ids)
        {
            team.Members.Add(new TeamMemberEntity
            {
                TeamId = team.Id,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            });
        }

        team.UpdatedAt = DateTime.UtcNow;
    }

    public async Task RemoveMembersAsync(
        string tenantId,
        string teamId,
        IReadOnlyCollection<string> memberIds,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(teamId)) throw new ArgumentNullException(nameof(teamId));
        if (memberIds is null || memberIds.Count == 0) return;

        tenantId = tenantId.Trim();
        teamId = teamId.Trim();

        var team = await _dbContext.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == teamId, cancellationToken);

        if (team is null) throw new InvalidOperationException("Team not found.");

        var ids = memberIds
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (ids.Count == 0) return;

        // ما بنسمح حذف الـ lead
        if (ids.Contains(team.TeamLeadId, StringComparer.Ordinal))
            throw new InvalidOperationException("Cannot remove the team lead.");

        var toRemove = team.Members.Where(m => ids.Contains(m.UserId)).ToList();
        if (toRemove.Count == 0) return;

        foreach (var row in toRemove)
            team.Members.Remove(row);

        team.UpdatedAt = DateTime.UtcNow;
    }

    // -----------------------
    // Read (Domain)
    // -----------------------

    public async Task<Team?> GetByIdAsync(string tenantId, string teamId, bool includeMembers = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(teamId))
            return null;

        tenantId = tenantId.Trim();
        teamId = teamId.Trim();

        var query = _dbContext.Teams.AsNoTracking().AsQueryable();

        if (includeMembers)
            query = query.Include(t => t.Members);

        var entity = await query
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == teamId, cancellationToken);

        return entity is null ? null : TeamMapper.ToDomain(entity);
    }

    public async Task<IReadOnlyList<Team>> GetByTenantAsync(string tenantId, bool includeMembers = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            return Array.Empty<Team>();

        tenantId = tenantId.Trim();

        var query = _dbContext.Teams.AsNoTracking().AsQueryable();

        if (includeMembers)
            query = query.Include(t => t.Members);

        var entities = await query
            .Where(t => t.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        return entities.Select(TeamMapper.ToDomain).ToList();
    }

    public async Task<PagedList<Team, DateTime?>> GetPagedAsync(
        TeamsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default)
    {
        parameters ??= new TeamsSearchAndFilterParametersDto();

        var query = _dbContext.Teams
            .AsNoTracking()
            .AsQueryable();

        query = ApplyFilters(query, parameters);

        if (parameters.Cursor.HasValue)
            query = query.Where(t => t.CreatedAt >= parameters.Cursor.Value);

        var entities = await query
            .OrderBy(t => t.CreatedAt)
            .Take(parameters.PageSize + 1)
            .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;
        if (entities.Count > parameters.PageSize)
        {
            nextCursor = entities.Last().CreatedAt;
            entities.RemoveAt(entities.Count - 1);
        }

        return new PagedList<Team, DateTime?>
        {
            Items = entities.Select(TeamMapper.ToDomain).ToList(),
            NextCursor = nextCursor
        };
    }

    // -----------------------
    // Rules checks
    // -----------------------

    public async Task<bool> IsNameUniqueAsync(string tenantId, string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) return false;
        if (string.IsNullOrWhiteSpace(name)) return false;

        tenantId = tenantId.Trim();
        var normalized = name.Trim().ToLowerInvariant();

        return !await _dbContext.Teams
            .AsNoTracking()
            .AnyAsync(t => t.TenantId == tenantId && t.Name.ToLower() == normalized, cancellationToken);
    }

    // -----------------------
    // Filters
    // -----------------------

    private static IQueryable<TeamEntity> ApplyFilters(IQueryable<TeamEntity> query, TeamsSearchAndFilterParametersDto parameters)
    {
        if (!string.IsNullOrWhiteSpace(parameters.TenantId))
            query = query.Where(t => t.TenantId == parameters.TenantId.Trim());

        if (!string.IsNullOrWhiteSpace(parameters.NameContains))
        {
            var term = parameters.NameContains.Trim();
            query = query.Where(t => t.Name.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(parameters.DescriptionContains))
        {
            var term = parameters.DescriptionContains.Trim();
            query = query.Where(t => t.Description != null && t.Description.Contains(term));
        }

        if (parameters.Department.HasValue)
            query = query.Where(t => t.Department == parameters.Department.Value);

        if (parameters.IsActive.HasValue)
            query = query.Where(t => t.IsActive == parameters.IsActive.Value);

        if (parameters.CapacityMin.HasValue)
            query = query.Where(t => t.Capacity >= parameters.CapacityMin.Value);

        if (parameters.CapacityMax.HasValue)
            query = query.Where(t => t.Capacity <= parameters.CapacityMax.Value);

        if (parameters.UpdatedAfter.HasValue)
            query = query.Where(t => t.UpdatedAt >= parameters.UpdatedAfter.Value);

        if (parameters.UpdatedBefore.HasValue)
            query = query.Where(t => t.UpdatedAt <= parameters.UpdatedBefore.Value);

        if (parameters.CreatedAfter.HasValue)
            query = query.Where(t => t.CreatedAt >= parameters.CreatedAfter.Value);

        if (parameters.CreatedBefore.HasValue)
            query = query.Where(t => t.CreatedAt <= parameters.CreatedBefore.Value);

        return query;
    }
}