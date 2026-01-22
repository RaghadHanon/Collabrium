using Application.Common;
using Application.Features.Teams.DTOs;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class TeamsRepository : ITeamsRepository
{
    private readonly AppDbContext _dbContext;

    public TeamsRepository(AppDbContext context)
    {
        _dbContext = context;
    }

    public async Task AddTeamAsync(Team team, CancellationToken cancellationToken = default)
    {
        await _dbContext.Teams.AddAsync(team, cancellationToken);
    }

    public async Task<Team?> GetByIdAsync(string tenantId, string teamId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Teams
            .Include(t => t.TeamLead)
            .Include(t => t.Members)
            .Include(t => t.Projects)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == teamId, cancellationToken);
    }

    public async Task<Team?> GetTeamByIdAsync(string tenantId, string id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Teams
            .Include(t => t.TeamLead)
            .Include(t => t.Members)
            .Include(t => t.Projects)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Team>> GetTeamsByTenantIdAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Teams
            .Where(t => t.TenantId == tenantId)
            .Include(t => t.TeamLead)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsTeamNameUniqueAsync(string tenantId, string name, CancellationToken cancellationToken = default)
    {
        var exists = await _dbContext.Teams.AnyAsync(
            t => t.TenantId == tenantId && t.Name.ToLower() == name.ToLower(),
            cancellationToken);

        return !exists;
    }

    public async Task<bool> RemoveTeamAsync(string id, CancellationToken cancellationToken = default)
    {
        var team = await _dbContext.Teams.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (team == null)
            return false;

        _dbContext.Teams.Remove(team);
        return true;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedList<Team, DateTime?>> GetTeamsAsync(
        TeamsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Teams
            .Include(t => t.TeamLead)
            .Include(t => t.Members)
            .Include(t => t.Projects)
            .AsNoTracking()
            .AsQueryable();

        query = ApplyFilters(query, parameters);
        return await ApplyPagination(parameters, query, cancellationToken);
    }

    private static async Task<PagedList<Team, DateTime?>> ApplyPagination(
       TeamsSearchAndFilterParametersDto parameters, IQueryable<Team> query,
       CancellationToken cancellationToken)
    {
        if (parameters.Cursor.HasValue)
            query = query.Where(c => c.CreatedAt >= parameters.Cursor.Value);

        var items = await query
            .Take(parameters.PageSize + 1)
            .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;
        if (items.Count > parameters.PageSize)
        {
            nextCursor = items.Last().CreatedAt;
            items.RemoveAt(items.Count - 1);
        }

        return new PagedList<Team, DateTime?>
        {
            Items = items,
            NextCursor = nextCursor
        };
    }

    private static IQueryable<Team> ApplyFilters(
        IQueryable<Team> query,
        TeamsSearchAndFilterParametersDto parameters)
    {
        if (!string.IsNullOrWhiteSpace(parameters.TenantId))
            query = query.Where(t => t.TenantId == parameters.TenantId);

        if (!string.IsNullOrWhiteSpace(parameters.TeamLeadName))
            query = query.Where(t => t.TeamLead.FullName.ToLower() == parameters.TeamLeadName.ToLower());

        if (!string.IsNullOrWhiteSpace(parameters.NameContains))
            query = query.Where(t => t.Name.Contains(parameters.NameContains));

        if (!string.IsNullOrWhiteSpace(parameters.DescriptionContains))
            query = query.Where(t => t.Description != null &&
                                     t.Description.Contains(parameters.DescriptionContains));

        if (parameters.Department.HasValue)
            query = query.Where(t => t.Department == parameters.Department.Value);

        if (parameters.IsActive.HasValue)
            query = query.Where(t => t.IsActive == parameters.IsActive.Value);

        if (parameters.CapacityMin.HasValue)
            query = query.Where(t => t.Capacity.HasValue && t.Capacity.Value >= parameters.CapacityMin.Value);

        if (parameters.CapacityMax.HasValue)
            query = query.Where(t => t.Capacity.HasValue && t.Capacity.Value <= parameters.CapacityMax.Value);

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

    public Task<Team?> GetByIdAsync(string teamId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Team?> GetTeamByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
