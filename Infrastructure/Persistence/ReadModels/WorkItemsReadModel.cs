using Application.Common;
using Application.Features.WorkItems.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Enums;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ReadModels;

public class WorkItemsReadModel : IWorkItemsReadModel
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public WorkItemsReadModel(AppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<WorkItemDto?> GetByIdAsync(
    string tenantId,
    string projectId,
    string workItemId,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(projectId) ||
            string.IsNullOrWhiteSpace(workItemId))
            return null;

        var tId = tenantId.Trim();
        var pId = projectId.Trim();
        var wId = workItemId.Trim();

        var entity = await _dbContext.WorkItems
            .AsNoTracking()
            .Where(w => w.Project.TenantId == tId && w.ProjectId == pId && w.Id == wId)
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null ? null : _mapper.Map<WorkItemDto>(entity);
    }

    public async Task<List<WorkItemListItemDto>> GetBoardItemsAsync(
        string tenantId,
        string projectId,
        string? sprintId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(projectId))
            return new List<WorkItemListItemDto>();

        var tId = tenantId.Trim();
        var pId = projectId.Trim();
        var sId = sprintId?.Trim();

        var query = _dbContext.WorkItems
            .AsNoTracking()
            .Where(w => w.Project.TenantId == tId && w.ProjectId == pId);

        if (!string.IsNullOrWhiteSpace(sId))
            query = query.Where(w => w.SprintId == sId);

        var items = await query
            .OrderBy(x => x.CreatedAt)
            .Where(x => x.Type != WorkItemType.UserStory && x.Type != WorkItemType.Epic && x.SprintId != null)
            .ProjectTo<WorkItemListItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return items;
    }

    public async Task<List<WorkItemListItemDto>> GetWorkItemsByTypeAsync(
            string tenantId,
            string projectId,
            WorkItemType type,
            string? sprintId,
            CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(projectId))
            return new List<WorkItemListItemDto>();

        var tId = tenantId.Trim();
        var pId = projectId.Trim();
        var sId = sprintId?.Trim();

        var query = _dbContext.WorkItems
            .AsNoTracking()
            .Where(w => w.Project.TenantId == tId && w.ProjectId == pId);

        if (!string.IsNullOrWhiteSpace(sId))
            query = query.Where(w => w.SprintId == sId);

        var items = await query
            .OrderBy(x => x.CreatedAt)
            .Where(x => x.Type == type)
            .ProjectTo<WorkItemListItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return items;
    }

    public async Task<PagedList<WorkItemListItemDto, DateTime?>> GetWorkItemsAsync(
        WorkItemsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default)
    {
        var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

        var query = _dbContext.WorkItems
            .Include(i => i.Project)
            .AsNoTracking();

        query = ApplyFilters(query, parameters);

        if (parameters.Cursor.HasValue)
            query = query.Where(x => x.CreatedAt >= parameters.Cursor.Value);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .ProjectTo<WorkItemListItemDto>(_mapper.ConfigurationProvider)
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;
        if (items.Count > pageSize)
        {
            nextCursor = items[^1].CreatedAt;
            items.RemoveAt(items.Count - 1);
        }

        return new PagedList<WorkItemListItemDto, DateTime?>
        {
            Items = items,
            NextCursor = nextCursor
        };
    }

    private static IQueryable<WorkItemEntity> ApplyFilters(IQueryable<WorkItemEntity> query,
        WorkItemsSearchAndFilterParametersDto parameters)
    {
        if (!string.IsNullOrWhiteSpace(parameters.TenantId))
        {
            var tId = parameters.TenantId.Trim();
            query = query.Where(x => x.Project.TenantId == tId);
        }

        if (!string.IsNullOrWhiteSpace(parameters.ProjectId))
        {
            var prId = parameters.ProjectId.Trim();
            query = query.Where(x => x.ProjectId == prId);
        }

        if (parameters.Type.HasValue)
            query = query.Where(x => x.Type == parameters.Type.Value);

        if (parameters.Status.HasValue)
            query = query.Where(x => x.Status == parameters.Status.Value);

        if (parameters.Priority.HasValue)
            query = query.Where(x => x.Priority == parameters.Priority.Value);

        if (!string.IsNullOrWhiteSpace(parameters.SprintId))
        {
            var sId = parameters.SprintId.Trim();
            query = query.Where(x => x.SprintId == sId);
        }

        if (!string.IsNullOrWhiteSpace(parameters.AssignedToUserId))
        {
            var aId = parameters.AssignedToUserId.Trim();
            query = query.Where(x => x.AssignedToUserId == aId);
        }

        if (!string.IsNullOrWhiteSpace(parameters.TitleContains))
        {
            var term = parameters.TitleContains.Trim();
            query = query.Where(x => x.Title.Contains(term));
        }

        if (parameters.CreatedAfter.HasValue)
            query = query.Where(x => x.CreatedAt >= parameters.CreatedAfter.Value);

        if (parameters.CreatedBefore.HasValue)
            query = query.Where(x => x.CreatedAt <= parameters.CreatedBefore.Value);

        return query;
    }
}