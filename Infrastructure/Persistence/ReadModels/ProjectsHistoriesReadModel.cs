using Application.Common;
using Application.Features.ProjectHistories.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ReadModels;

public sealed class ProjectsHistoriesReadModel : IProjectHistoriesReadModel
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public ProjectsHistoriesReadModel(AppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PagedList<ProjectHistoryDto, DateTime?>> GetPagedListAsync(
        ProjectHistorySearchAndFiltersParametersDto parameters,
        string projectId,
        string tenantId,
        CancellationToken ct = default)
    {
        parameters ??= new ProjectHistorySearchAndFiltersParametersDto();
        var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

        var query = _dbContext.ProjectHistories.AsNoTracking();
        query = ApplyFilters(query, parameters, tenantId, projectId);

        if (parameters.Cursor.HasValue)
            query = query.Where(x => x.CreatedAt <= parameters.Cursor.Value);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .ProjectTo<ProjectHistoryDto>(_mapper.ConfigurationProvider)
            .Take(pageSize + 1)
            .ToListAsync(ct);

        DateTime? nextCursor = null;
        if (items.Count > pageSize)
        {
            nextCursor = items[^1].CreatedAt;
            items.RemoveAt(items.Count - 1);
        }

        return new PagedList<ProjectHistoryDto, DateTime?>
        {
            Items = items,
            NextCursor = nextCursor
        };
    }

    private static IQueryable<ProjectHistoryEntity> ApplyFilters(
        IQueryable<ProjectHistoryEntity> query,
        ProjectHistorySearchAndFiltersParametersDto parameters,
        string tenantId,
        string projectId)
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