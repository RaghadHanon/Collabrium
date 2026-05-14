using Application.Common;
using Application.Features.ProjectsHistories.DTOs;
using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ProjectHistoryRepository : IProjectHistoryRepository
{
    private readonly AppDbContext _dbContext;

    public ProjectHistoryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public void Add(ProjectHistory history, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(history);

        _dbContext.ProjectHistories.Add(ProjectHistoryMapper.ToEntity(history));
    }

    public async Task<IReadOnlyList<ProjectHistory>> GetByProjectIdAsync(
        string tenantId,
        string projectId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(projectId))
            return [];

        var tId = tenantId.Trim();
        var pId = projectId.Trim();

        var entities = await _dbContext.ProjectHistories
            .AsNoTracking()
            .Where(x => x.Project.TenantId == tId && x.ProjectId == pId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(ProjectHistoryMapper.ToDomain).ToList();
    }

    public async Task<PagedList<ProjectHistory, DateTime?>> GetPagedAsync(
        ProjectHistorySearchAndFiltersParametersDto parameters,
        string projectId,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

        var query = _dbContext.ProjectHistories.AsNoTracking();

        query = ApplyFilters(query, parameters, tenantId, projectId);

        if (parameters.Cursor.HasValue)
            query = query.Where(x => x.CreatedAt < parameters.Cursor.Value);

        var items = await query
            .OrderBy(x => x.CreatedAt)
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;
        var hasMore = items.Count > pageSize;

        if (hasMore)
        {
            items.RemoveAt(items.Count - 1);
            nextCursor = items[^1].CreatedAt;
        }

        return new PagedList<ProjectHistory, DateTime?>
        {
            Items = items.Select(ProjectHistoryMapper.ToDomain).ToList(),
            NextCursor = nextCursor
        };
    }

    private static IQueryable<ProjectHistoryEntity> ApplyFilters(
        IQueryable<ProjectHistoryEntity> query,
        ProjectHistorySearchAndFiltersParametersDto parameters,
        string projectId,
        string tenantId)
    {
        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            var tId = tenantId.Trim();
            query = query.Where(x => x.Project.TenantId == tId);
        }

        if (!string.IsNullOrWhiteSpace(projectId))
        {
            var pId = projectId.Trim();
            query = query.Where(x => x.ProjectId == pId);
        }

        if (!string.IsNullOrWhiteSpace(parameters.MadeByUserId))
        {
            var uId = parameters.MadeByUserId.Trim();
            query = query.Where(x => x.MadeByUserId == uId);
        }

        if (!string.IsNullOrWhiteSpace(parameters.PropertyName))
        {
            var prop = parameters.PropertyName.Trim();
            query = query.Where(x => x.PropertyName == prop);
        }

        if (!string.IsNullOrWhiteSpace(parameters.RelatedEntityTitle))
        {
            var title = parameters.RelatedEntityTitle.Trim();
            query = query.Where(x => x.RelatedEntityTitle == title);
        }

        if (parameters.CreatedAfter.HasValue)
            query = query.Where(x => x.CreatedAt >= parameters.CreatedAfter.Value);

        if (parameters.CreatedBefore.HasValue)
            query = query.Where(x => x.CreatedAt <= parameters.CreatedBefore.Value);

        if (parameters.Scope.HasValue)
            query = query.Where(x => x.Scope == parameters.Scope);

        if (parameters.ActionType.HasValue)
            query = query.Where(x => x.ActionType == parameters.ActionType);

        return query;
    }
}