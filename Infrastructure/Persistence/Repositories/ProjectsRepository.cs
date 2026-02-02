using Application.Common;
using Application.Common.DTOs;
using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class ProjectsRepository : IProjectsRepository
{
    private readonly AppDbContext _dbContext;

    public ProjectsRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(project);
        await _dbContext.Projects.AddAsync(ProjectMapper.ToEntity(project), cancellationToken);
    }

    public async Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(project);

        var entity = await _dbContext.Projects
            .FirstOrDefaultAsync(p => p.Id == project.Id, cancellationToken);

        if (entity is null)
            throw new InvalidOperationException($"Project '{project.Id}' was not found.");

        ProjectMapper.UpdateEntity(project, entity);
    }

    public async Task<bool> RemoveAsync(string tenantId, string projectId, CancellationToken cancellationToken = default)
    {
        if (!TryNormalize(tenantId, projectId, out var tId, out var pId))
            return false;

        var entity = await _dbContext.Projects
            .FirstOrDefaultAsync(p => p.TenantId == tId && p.Id == pId, cancellationToken);

        if (entity is null) return false;

        _dbContext.Projects.Remove(entity);
        return true;
    }

    public async Task<Project?> GetByIdAsync(string tenantId, string projectId, CancellationToken cancellationToken = default)
    {
        if (!TryNormalize(tenantId, projectId, out var tId, out var pId))
            return null;

        var entity = await _dbContext.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.TenantId == tId && p.Id == pId, cancellationToken);

        return entity is null ? null : ProjectMapper.ToDomain(entity);
    }

    public async Task<bool> IsNameUniqueAsync(string tenantId, string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(name))
            return false;

        var tId = tenantId.Trim();
        var normalized = name.Trim().ToLowerInvariant();

        return !await _dbContext.Projects
            .AsNoTracking()
            .AnyAsync(p => p.TenantId == tId && p.Name.ToLower() == normalized, cancellationToken);
    }

    public async Task<PagedList<Project, DateTime?>> GetPagedAsync(
        string tenantId,
        QueryParametersDto<DateTime?> parameters,
        CancellationToken cancellationToken = default)
    {
        var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

        if (string.IsNullOrWhiteSpace(tenantId))
            return new PagedList<Project, DateTime?> { Items = new(), NextCursor = null };

        var tId = tenantId.Trim();

        var query = _dbContext.Projects.AsNoTracking()
            .Where(p => p.TenantId == tId);

        if (parameters.Cursor.HasValue)
            query = query.Where(p => p.CreatedAt > parameters.Cursor.Value);

        var entities = await query
            .OrderBy(p => p.CreatedAt)
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;
        if (entities.Count > pageSize)
        {
            nextCursor = entities[^1].CreatedAt;
            entities.RemoveAt(entities.Count - 1);
        }

        return new PagedList<Project, DateTime?>
        {
            Items = entities.Select(ProjectMapper.ToDomain).ToList(),
            NextCursor = nextCursor
        };
    }

    private static bool TryNormalize(string? tenantId, string? projectId, out string tId, out string pId)
    {
        tId = tenantId?.Trim() ?? "";
        pId = projectId?.Trim() ?? "";
        return !string.IsNullOrWhiteSpace(tId) && !string.IsNullOrWhiteSpace(pId);
    }
}