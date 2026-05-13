using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class TeamsRepository : ITeamsRepository
{
    private readonly AppDbContext _dbContext;

    public TeamsRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddAsync(Team team, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(team);

        var entity = TeamMapper.ToEntity(team);

        entity.Members.Add(CreateMemberRow(entity.Id, team.TeamLeadId));

        await _dbContext.Teams.AddAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(Team team, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(team);

        var entity = await _dbContext.Teams
            .FirstOrDefaultAsync(t => t.Id == team.Id, cancellationToken);

        if (entity is null)
            throw new InvalidOperationException("Team not found.");

        TeamMapper.UpdateEntity(team, entity);
    }

    public async Task<bool> RemoveAsync(string tenantId, string teamId, CancellationToken cancellationToken = default)
    {
        if (!TryNormalize(tenantId, teamId, out var tId, out var tmId))
            return false;

        var entity = await _dbContext.Teams
            .FirstOrDefaultAsync(t => t.TenantId == tId && t.Id == tmId, cancellationToken);

        if (entity is null) return false;

        _dbContext.Teams.Remove(entity);
        return true;
    }

    public async Task<Team?> GetByIdAsync(string tenantId, string teamId, CancellationToken cancellationToken = default)
    {
        if (!TryNormalize(tenantId, teamId, out var tId, out var tmId))
            return null;

        var entity = await _dbContext.Teams
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TenantId == tId && t.Id == tmId, cancellationToken);

        return entity is null ? null : TeamMapper.ToDomain(entity);
    }

    public async Task AddMembersAsync(
        string tenantId,
        string teamId,
        IReadOnlyCollection<string> memberIds,
        CancellationToken cancellationToken = default)
    {
        if (!TryNormalize(tenantId, teamId, out var tId, out var tmId))
            throw new ArgumentNullException("tenantId/teamId");

        var ids = NormalizeIds(memberIds);
        if (ids.Count == 0) return;

        var teamEntity = await _dbContext.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.TenantId == tId && t.Id == tmId, cancellationToken);

        if (teamEntity is null)
            throw new InvalidOperationException("Team not found.");

        await EnsureUsersExistInTenantAsync(tId, ids, cancellationToken);

        var teamDomain = TeamMapper.ToDomain(teamEntity);
        foreach (var id in ids)
            teamDomain.AddMember(id);

        var existing = teamEntity.Members
            .Select(m => m.UserId)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var id in ids)
        {
            if (existing.Contains(id)) continue;
            teamEntity.Members.Add(CreateMemberRow(teamEntity.Id, id));
        }

        TeamMapper.UpdateEntity(teamDomain, teamEntity);
    }

    public async Task RemoveMembersAsync(
        string tenantId,
        string teamId,
        IReadOnlyCollection<string> memberIds,
        CancellationToken cancellationToken = default)
    {
        if (!TryNormalize(tenantId, teamId, out var tId, out var tmId))
            throw new ArgumentNullException("tenantId/teamId");

        var ids = NormalizeIds(memberIds);
        if (ids.Count == 0) return;

        var teamEntity = await _dbContext.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.TenantId == tId && t.Id == tmId, cancellationToken);

        if (teamEntity is null)
            throw new InvalidOperationException("Team not found.");

        var teamDomain = TeamMapper.ToDomain(teamEntity);
        foreach (var id in ids)
            teamDomain.RemoveMember(id);

        var toRemove = teamEntity.Members
            .Where(m => ids.Contains(m.UserId, StringComparer.Ordinal))
            .ToList();

        foreach (var row in toRemove)
            teamEntity.Members.Remove(row);

        TeamMapper.UpdateEntity(teamDomain, teamEntity);
    }

    public async Task<bool> IsNameUniqueAsync(string tenantId, string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) return false;
        if (string.IsNullOrWhiteSpace(name)) return false;

        var tId = tenantId.Trim();
        var normalized = name.Trim().ToLowerInvariant();

        return !await _dbContext.Teams
            .AsNoTracking()
            .AnyAsync(t => t.TenantId == tId && t.Name.ToLower() == normalized, cancellationToken);
    }

    private static TeamMemberEntity CreateMemberRow(string teamId, string userId)
    {
        return new()
        {
            TeamId = teamId,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        };
    }

    private static bool TryNormalize(string tenantId, string teamId, out string tId, out string tmId)
    {
        tId = tenantId?.Trim() ?? "";
        tmId = teamId?.Trim() ?? "";
        return !string.IsNullOrWhiteSpace(tId) && !string.IsNullOrWhiteSpace(tmId);
    }

    private static List<string> NormalizeIds(IReadOnlyCollection<string>? ids)
    {
        return ids is null || ids.Count == 0
            ? new()
            : ids
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private async Task EnsureUsersExistInTenantAsync(string tenantId, List<string> userIds, CancellationToken ct)
    {
        var users = await _dbContext.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.TenantId })
            .ToListAsync(ct);

        if (users.Count != userIds.Count)
            throw new InvalidOperationException("One or more users were not found.");

        if (users.Any(u => !string.Equals(u.TenantId, tenantId, StringComparison.Ordinal)))
            throw new InvalidOperationException("One or more users are not in the current tenant.");
    }

    public async Task<bool> IsTeamExists(string tenantId, string teamId)
    {
        return await _dbContext.Teams
            .AsNoTracking()
            .AnyAsync(t => t.TenantId == tenantId && t.Id == teamId);
    }
}