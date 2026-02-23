using Application.Common;
using Application.Features.Sprints.DTOs;
using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class SprintsRepository : ISprintsRepository
{
    private readonly AppDbContext _dbContext;

    public SprintsRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public void Add(Sprint sprint, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sprint);

        _dbContext.Sprints.Add(SprintMapper.ToEntity(sprint));
    }

    public async Task UpdateAsync(Sprint sprint, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sprint);

        var entity = await _dbContext.Sprints
            .FirstOrDefaultAsync(s => s.Id == sprint.Id && s.TenantId == sprint.TenantId, cancellationToken);

        if (entity is null)
            throw new InvalidOperationException($"Sprint '{sprint.Id}' was not found.");

        SprintMapper.UpdateEntity(sprint, entity);
    }

    public async Task<bool> RemoveAsync(string tenantId, string sprintId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(sprintId))
            return false;

        var tId = tenantId.Trim();
        var sId = sprintId.Trim();

        var entity = await _dbContext.Sprints
            .FirstOrDefaultAsync(s => s.TenantId == tId && s.Id == sId, cancellationToken);

        if (entity is null) return false;

        _dbContext.Sprints.Remove(entity);
        return true;
    }

    public async Task<bool> IsExistsAsync(string tenantId, string sprintId, string? projectId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(sprintId))
            return false;

        var tId = tenantId.Trim();
        var sId = sprintId.Trim();

        return await _dbContext.Sprints
            .AsNoTracking()
            .AnyAsync(s => s.TenantId == tId && s.Id == sId && (projectId != null && s.ProjectId == projectId), cancellationToken);
    }

    public async Task<Sprint?> GetByIdAsync(string tenantId, string sprintId, string? projectId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(sprintId))
            return null;

        var tId = tenantId.Trim();
        var sId = sprintId.Trim();

        var entity = await _dbContext.Sprints
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tId && s.Id == sId && (projectId != null && s.ProjectId == projectId), cancellationToken);

        return entity is null ? null : SprintMapper.ToDomain(entity);
    }

    public Task<bool> HasActiveSprintAsync(string tenantId, string projectId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(projectId))
            return Task.FromResult(false);

        var tId = tenantId.Trim();
        var pId = projectId.Trim();

        return _dbContext.Sprints
            .AsNoTracking()
            .AnyAsync(s => s.TenantId == tId
                        && s.ProjectId == pId
                        && s.Status == Domain.Enums.SprintStatus.Active, cancellationToken);
    }

    public Task<bool> HasWorkItemsAsync(string tenantId, string sprintId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(sprintId))
            return Task.FromResult(false);

        var sId = sprintId.Trim();

        return _dbContext.WorkItems
            .AsNoTracking()
            .AnyAsync(w => w.SprintId == sId, cancellationToken);
    }

    public async Task<PagedList<Sprint, DateTime?>> GetPagedAsync(
        SprintsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default)
    {
        var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

        var query = _dbContext.Sprints.AsNoTracking();

        query = ApplyFilters(query, parameters);

        if (parameters.Cursor.HasValue)
            query = query.Where(x => x.CreatedAt >= parameters.Cursor.Value);

        var entities = await query
            .OrderBy(x => x.CreatedAt)
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;
        if (entities.Count > pageSize)
        {
            nextCursor = entities[^1].CreatedAt;
            entities.RemoveAt(entities.Count - 1);
        }

        return new PagedList<Sprint, DateTime?>
        {
            Items = entities.Select(SprintMapper.ToDomain).ToList(),
            NextCursor = nextCursor
        };
    }

    private static IQueryable<SprintEntity> ApplyFilters(IQueryable<SprintEntity> query, SprintsSearchAndFilterParametersDto p)
    {
        if (!string.IsNullOrWhiteSpace(p.TenantId))
        {
            var tId = p.TenantId.Trim();
            query = query.Where(x => x.TenantId == tId);
        }

        if (!string.IsNullOrWhiteSpace(p.ProjectId))
        {
            var prId = p.ProjectId.Trim();
            query = query.Where(x => x.ProjectId == prId);
        }

        if (!string.IsNullOrWhiteSpace(p.NameContains))
        {
            var term = p.NameContains.Trim();
            query = query.Where(x => x.Name.Contains(term));
        }

        if (p.Status.HasValue)
            query = query.Where(x => x.Status == p.Status.Value);

        if (p.StartAfter.HasValue)
            query = query.Where(x => x.StartDate >= p.StartAfter.Value);

        if (p.StartBefore.HasValue)
            query = query.Where(x => x.StartDate <= p.StartBefore.Value);

        if (p.EndAfter.HasValue)
            query = query.Where(x => x.EndDate >= p.EndAfter.Value);

        if (p.EndBefore.HasValue)
            query = query.Where(x => x.EndDate <= p.EndBefore.Value);

        if (p.CreatedAfter.HasValue)
            query = query.Where(x => x.CreatedAt >= p.CreatedAfter.Value);

        if (p.CreatedBefore.HasValue)
            query = query.Where(x => x.CreatedAt <= p.CreatedBefore.Value);

        return query;
    }
}