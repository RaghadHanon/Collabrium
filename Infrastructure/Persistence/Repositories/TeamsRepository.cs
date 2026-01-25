using Application.Common;
using Application.Features.Projects.DTOs;
using Application.Features.Teams.DTOs;
using Application.Features.Users.DTOs;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Models;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class TeamsRepository : ITeamsRepository
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public TeamsRepository(AppDbContext context, IMapper mapper)
    {
        _dbContext = context;
        _mapper = mapper;
    }

    public async Task AddTeamAsync(Team team, CancellationToken cancellationToken = default)
    {
        var entity = TeamMapper.ToEntity(team);

        // add leader as member by default
        var leader = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == team.TeamLeadId, cancellationToken);
        if (leader != null)
            entity.Members.Add(leader);

        await _dbContext.Teams.AddAsync(entity, cancellationToken);
    }

    public async Task<Team?> GetByIdAsync(string tenantId, string teamId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Teams
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == teamId, cancellationToken);

        return entity == null ? null : TeamMapper.ToDomain(entity);
    }

    public async Task<Team?> GetTeamByIdAsync(string tenantId, string id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Teams
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id, cancellationToken);

        return entity == null ? null : TeamMapper.ToDomain(entity);
    }

    public async Task<IReadOnlyList<Team>> GetTeamsByTenantIdAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.Teams
            .Where(t => t.TenantId == tenantId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return entities.Select(TeamMapper.ToDomain).ToList();
    }

    public async Task<PagedList<Team, DateTime?>> GetTeamsAsync(
        TeamsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Teams.AsNoTracking().AsQueryable();
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

    public async Task<bool> RemoveTeamAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Teams.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (entity == null) return false;

        _dbContext.Teams.Remove(entity);
        return true;
    }

    public async Task<bool> IsTeamNameUniqueAsync(string tenantId, string name, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToLower();
        return !await _dbContext.Teams.AnyAsync(
            t => t.TenantId == tenantId && t.Name.ToLower() == normalized,
            cancellationToken);
    }

    public async Task UpdateTeamAsync(Team team, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Teams.FirstOrDefaultAsync(t => t.Id == team.Id, cancellationToken);
        if (entity == null) return;

        // domain -> entity (do it centrally if you have TeamMapper.UpdateEntity)
        entity.TeamLeadId = team.TeamLeadId;
        entity.Name = team.Name;
        entity.Description = team.Description;
        entity.Department = team.Department;
        entity.Capacity = team.Capacity;
        entity.IsActive = team.IsActive;
        entity.UpdatedAt = team.UpdatedAt;
    }

    public async Task ToggleActivationAsync(string tenantId, string teamId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Teams
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == teamId, cancellationToken);

        if (entity == null) return;

        entity.IsActive = !entity.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
    }

    public async Task AddMembersAsync(
        string tenantId,
        string teamId,
        IReadOnlyCollection<string> memberIds,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == teamId, cancellationToken);

        if (entity == null)
            throw new InvalidOperationException("Team not found.");

        if (!entity.IsActive)
            throw new InvalidOperationException("Cannot add members to an inactive team.");

        var cleanIds = memberIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (cleanIds.Count == 0) return;

        var users = await _dbContext.Users
            .Where(u => cleanIds.Contains(u.Id))
            .ToListAsync(cancellationToken);

        if (users.Count != cleanIds.Count)
            throw new InvalidOperationException("One or more users were not found.");

        if (users.Any(u => u.TenantId != tenantId))
            throw new InvalidOperationException("One or more users are not in the current tenant.");

        var existing = entity.Members.Select(m => m.Id).ToHashSet(StringComparer.Ordinal);
        if (cleanIds.Any(id => existing.Contains(id)))
            throw new InvalidOperationException("One or more users are already members of the team.");

        if (entity.Capacity.HasValue && entity.Members.Count + users.Count > entity.Capacity.Value)
            throw new InvalidOperationException("Adding these members exceeds the team capacity.");

        foreach (var u in users)
            entity.Members.Add(u);

        entity.UpdatedAt = DateTime.UtcNow;
    }

    // ✅ DTO read query (fixed + uses AutoMapper correctly)
    public async Task<TeamFullInfoDto?> GetTeamFullAsync(
        string tenantId,
        string teamId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Teams
            .Include(t => t.TeamLead)
            .Include(t => t.Members)
            .Include(t => t.Projects)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == teamId, cancellationToken);

        if (entity == null) return null;

        // entity -> domain (for core team fields)
        var teamDomain = TeamMapper.ToDomain(entity);

        // base map (Team -> TeamFullInfoDto), then fill extra fields
        var dto = _mapper.Map<TeamFullInfoDto>(teamDomain);

        dto.TeamLeadName = entity.TeamLead.FullName;
        dto.Count = entity.Members.Count;

        dto.TeamLead = _mapper.Map<UserDto>(UserMapper.ToDomain(entity.TeamLead));
        dto.Members = entity.Members.Select(m => _mapper.Map<UserDto>(UserMapper.ToDomain(m))).ToList();
        dto.Projects = entity.Projects.Select(p => _mapper.Map<ProjectDto>(ProjectMapper.ToDomain(p))).ToList();

        return dto;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<TeamEntity> ApplyFilters(IQueryable<TeamEntity> query, TeamsSearchAndFilterParametersDto parameters)
    {
        if (!string.IsNullOrWhiteSpace(parameters.TenantId))
            query = query.Where(t => t.TenantId == parameters.TenantId);

        if (!string.IsNullOrWhiteSpace(parameters.NameContains))
            query = query.Where(t => t.Name.Contains(parameters.NameContains));

        if (!string.IsNullOrWhiteSpace(parameters.DescriptionContains))
            query = query.Where(t => t.Description != null && t.Description.Contains(parameters.DescriptionContains));

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