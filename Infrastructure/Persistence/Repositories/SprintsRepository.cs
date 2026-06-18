using Application.Common;
using Application.Features.Sprints.DTOs;
using Application.Interfaces.Repositories;
using Domain.Enums;
using Domain.Models;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class SprintsRepository : ISprintsRepository
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 50;

    private readonly AppDbContext _dbContext;

    public SprintsRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext
            ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public void Add(
        Sprint sprint,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sprint);

        _dbContext.Sprints.Add(
            SprintMapper.ToEntity(sprint));
    }

    public async Task UpdateAsync(
        Sprint sprint,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sprint);

        var entity = await _dbContext.Sprints
            .FirstOrDefaultAsync(
                current =>
                    current.Id == sprint.Id &&
                    current.TenantId == sprint.TenantId &&
                    current.ProjectId == sprint.ProjectId,
                cancellationToken);

        if (entity is null)
        {
            throw new InvalidOperationException(
                $"Sprint '{sprint.Id}' was not found.");
        }

        SprintMapper.UpdateEntity(
            sprint,
            entity);
    }

    public async Task<bool> RemoveAsync(
        string tenantId,
        string sprintId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(sprintId))
        {
            return false;
        }

        var tId = tenantId.Trim();
        var sId = sprintId.Trim();

        var entity = await _dbContext.Sprints
            .FirstOrDefaultAsync(
                sprint =>
                    sprint.TenantId == tId &&
                    sprint.Id == sId,
                cancellationToken);

        if (entity is null)
            return false;

        _dbContext.Sprints.Remove(entity);

        return true;
    }

    public async Task<bool> IsExistsAsync(
        string tenantId,
        string sprintId,
        string? projectId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(sprintId))
        {
            return false;
        }

        var tId = tenantId.Trim();
        var sId = sprintId.Trim();
        var pId = projectId?.Trim();

        var query = _dbContext.Sprints
            .AsNoTracking()
            .Where(sprint =>
                sprint.TenantId == tId &&
                sprint.Id == sId);

        if (!string.IsNullOrWhiteSpace(pId))
        {
            query = query.Where(sprint =>
                sprint.ProjectId == pId);
        }

        return await query.AnyAsync(
            cancellationToken);
    }

    public async Task<Sprint?> GetByIdAsync(
        string tenantId,
        string? sprintId,
        string? projectId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(sprintId))
        {
            return null;
        }

        var tId = tenantId.Trim();
        var sId = sprintId.Trim();
        var pId = projectId?.Trim();

        var query = _dbContext.Sprints
            .AsNoTracking()
            .Where(sprint =>
                sprint.TenantId == tId &&
                sprint.Id == sId);

        if (!string.IsNullOrWhiteSpace(pId))
        {
            query = query.Where(sprint =>
                sprint.ProjectId == pId);
        }

        var entity = await query.FirstOrDefaultAsync(
            cancellationToken);

        return entity is null
            ? null
            : SprintMapper.ToDomain(entity);
    }

    public Task<bool> ProjectHasActiveSprintAsync(
        string tenantId,
        string projectId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(projectId))
        {
            return Task.FromResult(false);
        }

        var tId = tenantId.Trim();
        var pId = projectId.Trim();

        return _dbContext.Sprints
            .AsNoTracking()
            .AnyAsync(
                sprint =>
                    sprint.TenantId == tId &&
                    sprint.ProjectId == pId &&
                    sprint.Status == SprintStatus.Active,
                cancellationToken);
    }

    public Task<bool> SprintHasWorkItemsAsync(
        string tenantId,
        string sprintId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(sprintId))
        {
            return Task.FromResult(false);
        }

        var tId = tenantId.Trim();
        var sId = sprintId.Trim();

        return _dbContext.WorkItems
            .AsNoTracking()
            .AnyAsync(
                workItem =>
                    workItem.SprintId == sId &&
                    workItem.Project.TenantId == tId,
                cancellationToken);
    }

    public async Task<bool> CanSprintBeCompletedAsync(
        string tenantId,
        string sprintId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(sprintId))
        {
            return false;
        }

        var tId = tenantId.Trim();
        var sId = sprintId.Trim();

        var hasIncompleteWorkItems =
            await _dbContext.WorkItems
                .AsNoTracking()
                .AnyAsync(
                    workItem =>
                        workItem.SprintId == sId &&
                        workItem.Project.TenantId == tId &&
                        workItem.Status != WorkItemStatus.Done,
                    cancellationToken);

        return !hasIncompleteWorkItems;
    }

    public async Task<List<WorkItemEntity>?> GetWorkItemsAsync(
        string tenantId,
        string sprintId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(sprintId))
        {
            return null;
        }

        var tId = tenantId.Trim();
        var sId = sprintId.Trim();

        return await _dbContext.WorkItems
            .AsNoTracking()
            .Where(workItem =>
                workItem.SprintId == sId &&
                workItem.Project.TenantId == tId)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedList<Sprint, DateTime?>> GetPagedAsync(
        SprintsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var requestedPageSize =
            parameters.PageSize <= 0
                ? DefaultPageSize
                : parameters.PageSize;

        var pageSize = Math.Min(
            requestedPageSize,
            MaxPageSize);

        var query = _dbContext.Sprints
            .AsNoTracking()
            .AsQueryable();

        query = ApplyFilters(
            query,
            parameters);

        if (parameters.Cursor.HasValue)
        {
            query = query.Where(sprint =>
                sprint.CreatedAt > parameters.Cursor.Value);
        }

        var entities = await query
            .OrderBy(sprint => sprint.CreatedAt)
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;

        if (entities.Count > pageSize)
        {
            entities.RemoveAt(entities.Count - 1);

            nextCursor = entities.Count > 0
                ? entities[^1].CreatedAt
                : null;
        }

        return new PagedList<Sprint, DateTime?>
        {
            Items = entities
                .Select(SprintMapper.ToDomain)
                .ToList(),

            NextCursor = nextCursor
        };
    }

    private static IQueryable<SprintEntity> ApplyFilters(
        IQueryable<SprintEntity> query,
        SprintsSearchAndFilterParametersDto parameters)
    {
        if (!string.IsNullOrWhiteSpace(parameters.TenantId))
        {
            var tenantId = parameters.TenantId.Trim();

            query = query.Where(sprint =>
                sprint.TenantId == tenantId);
        }

        if (!string.IsNullOrWhiteSpace(parameters.ProjectId))
        {
            var projectId = parameters.ProjectId.Trim();

            query = query.Where(sprint =>
                sprint.ProjectId == projectId);
        }

        if (!string.IsNullOrWhiteSpace(parameters.NameContains))
        {
            var term = parameters.NameContains.Trim();

            query = query.Where(sprint =>
                sprint.Name != null &&
                sprint.Name.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Code))
        {
            var code = parameters.Code.Trim();

            query = query.Where(sprint =>
                sprint.Code == code);
        }

        if (parameters.Status.HasValue)
        {
            query = query.Where(sprint =>
                sprint.Status == parameters.Status.Value);
        }

        if (parameters.StartAfter.HasValue)
        {
            query = query.Where(sprint =>
                sprint.PlannedStartDate.HasValue &&
                sprint.PlannedStartDate.Value >= parameters.StartAfter.Value);
        }

        if (parameters.StartBefore.HasValue)
        {
            query = query.Where(sprint =>
                sprint.PlannedStartDate.HasValue &&
                sprint.PlannedStartDate.Value <= parameters.StartBefore.Value);
        }

        if (parameters.EndAfter.HasValue)
        {
            query = query.Where(sprint =>
                sprint.PlannedEndDate.HasValue &&
                sprint.PlannedEndDate.Value >= parameters.EndAfter.Value);
        }

        if (parameters.EndBefore.HasValue)
        {
            query = query.Where(sprint =>
                sprint.PlannedEndDate.HasValue &&
                sprint.PlannedEndDate.Value <= parameters.EndBefore.Value);
        }

        if (parameters.StartedAfter.HasValue)
        {
            query = query.Where(sprint =>
                sprint.StartedAt.HasValue &&
                sprint.StartedAt.Value >= parameters.StartedAfter.Value);
        }

        if (parameters.StartedBefore.HasValue)
        {
            query = query.Where(sprint =>
                sprint.StartedAt.HasValue &&
                sprint.StartedAt.Value <= parameters.StartedBefore.Value);
        }

        if (parameters.CompletedAfter.HasValue)
        {
            query = query.Where(sprint =>
                sprint.CompletedAt.HasValue &&
                sprint.CompletedAt.Value >=
                    parameters.CompletedAfter.Value);
        }

        if (parameters.CompletedBefore.HasValue)
        {
            query = query.Where(sprint =>
                sprint.CompletedAt.HasValue &&
                sprint.CompletedAt.Value <=
                    parameters.CompletedBefore.Value);
        }

        if (parameters.CreatedAfter.HasValue)
        {
            query = query.Where(sprint =>
                sprint.CreatedAt >= parameters.CreatedAfter.Value);
        }

        if (parameters.CreatedBefore.HasValue)
        {
            query = query.Where(sprint =>
                sprint.CreatedAt <= parameters.CreatedBefore.Value);
        }

        return query;
    }
}