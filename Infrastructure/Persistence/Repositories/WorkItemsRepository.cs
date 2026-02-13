using Application.Common;
using Application.Features.WorkItems.DTOs;
using Application.Interfaces.Repositories;
using AutoMapper.QueryableExtensions;
using Domain.Models;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class WorkItemsRepository : IWorkItemsRepository
{
    private readonly AppDbContext _dbContext;

    public WorkItemsRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public void Add(WorkItem workItem, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workItem);

        _dbContext.WorkItems.Add(WorkItemMapper.ToEntity(workItem));
    }

    public async Task UpdateAsync(WorkItem workItem, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workItem);

        var entity = await _dbContext.WorkItems
            .Include(w => w.Comments)
            .Include(w => w.Relations)
            .Include(w => w.CodeLinks)
            .Include(w => w.Attachments)
            .FirstOrDefaultAsync(w => w.Id == workItem.Id, cancellationToken);

        if (entity is null)
            throw new InvalidOperationException($"WorkItem '{workItem.Id}' was not found.");

        WorkItemMapper.UpdateEntity(workItem, entity);
    }

    public async Task<bool> RemoveAsync(string tenantId, string projectId, string workItemId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(workItemId))
            return false;

        var tId = tenantId.Trim();
        var wId = workItemId.Trim();

        var entity = await _dbContext.WorkItems
            .Include(w => w.Project)
            .FirstOrDefaultAsync(w => w.Project.TenantId == tId && w.Id == wId && w.ProjectId == projectId, cancellationToken);

        if (entity is null) return false;

        _dbContext.WorkItems.Remove(entity);
        return true;
    }

    public async Task<WorkItem?> GetByIdAsync(string tenantId, string projectId, string workItemId, bool includeChildren = true, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(workItemId))
            return null;

        var tId = tenantId.Trim();
        var wId = workItemId.Trim();

        var query = _dbContext.WorkItems.AsNoTracking();

        if (includeChildren)
        {
            query = query
                .Include(w => w.Project)
                .Include(w => w.Comments)
                .Include(w => w.Relations)
                .Include(w => w.CodeLinks)
                .Include(w => w.Attachments);
        }

        var entity = await query.FirstOrDefaultAsync(w => w.Project.TenantId == tId && w.Id == wId && w.ProjectId == projectId, cancellationToken);

        return entity is null ? null : WorkItemMapper.ToDomain(entity, includeChildren);
    }

    public async Task<PagedList<WorkItem, DateTime?>> GetPagedAsync(
        WorkItemsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default)
    {
        var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

        var query = _dbContext.WorkItems.AsNoTracking();

        query = ApplyFilters(query, parameters);

        if (parameters.Cursor.HasValue)
            query = query.Where(x => x.CreatedAt >= parameters.Cursor.Value);

        var entities = await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;
        if (entities.Count > pageSize)
        {
            nextCursor = entities[^1].CreatedAt;
            entities.RemoveAt(entities.Count - 1);
        }

        return new PagedList<WorkItem, DateTime?>
        {
            Items = entities.Select(e => WorkItemMapper.ToDomain(e, includeChildren: false)).ToList(),
            NextCursor = nextCursor
        };
    }

    private static IQueryable<WorkItemEntity> ApplyFilters(IQueryable<WorkItemEntity> query, WorkItemsSearchAndFilterParametersDto p)
    {
        if (!string.IsNullOrWhiteSpace(p.TenantId))
        {
            var tId = p.TenantId.Trim();
            query = query.Where(x => x.Project.TenantId == tId);
        }

        if (!string.IsNullOrWhiteSpace(p.ProjectId))
        {
            var prId = p.ProjectId.Trim();
            query = query.Where(x => x.ProjectId == prId);
        }

        if (p.Type.HasValue)
            query = query.Where(x => x.Type == p.Type.Value);

        if (p.Status.HasValue)
            query = query.Where(x => x.Status == p.Status.Value);

        if (p.Priority.HasValue)
            query = query.Where(x => x.Priority == p.Priority.Value);

        if (!string.IsNullOrWhiteSpace(p.SprintId))
        {
            var sId = p.SprintId.Trim();
            query = query.Where(x => x.SprintId == sId);
        }

        if (!string.IsNullOrWhiteSpace(p.AssignedToUserId))
        {
            var aId = p.AssignedToUserId.Trim();
            query = query.Where(x => x.AssignedToUserId == aId);
        }

        if (!string.IsNullOrWhiteSpace(p.TitleContains))
        {
            var term = p.TitleContains.Trim();
            query = query.Where(x => x.Title.Contains(term));
        }

        if (p.CreatedAfter.HasValue)
            query = query.Where(x => x.CreatedAt >= p.CreatedAfter.Value);

        if (p.CreatedBefore.HasValue)
            query = query.Where(x => x.CreatedAt <= p.CreatedBefore.Value);

        return query;
    }
}