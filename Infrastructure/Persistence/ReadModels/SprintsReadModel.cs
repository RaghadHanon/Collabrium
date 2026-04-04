using Application.Common;
using Application.Features.Sprints.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ReadModels;

public sealed class SprintsReadModel : ISprintsReadModel
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public SprintsReadModel(AppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<SprintDto?> GetByIdAsync(string tenantId, string? sprintId, string? projectId = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(sprintId))
            return null;

        var tId = tenantId.Trim();
        var sId = sprintId.Trim();
        var prId = projectId?.Trim();

        return await _dbContext.Sprints
            .AsNoTracking()
            .Where(s => s.TenantId == tId && s.Id == sId && (projectId != null && s.ProjectId == projectId))
            .ProjectTo<SprintDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PagedList<SprintDto, DateTime?>> GetSprintsAsync(
        SprintsSearchAndFilterParametersDto parameters,
        CancellationToken ct = default)
    {
        parameters ??= new SprintsSearchAndFilterParametersDto();
        var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

        var query = _dbContext.Sprints.AsNoTracking();

        query = ApplyFilters(query, parameters);

        if (parameters.Cursor.HasValue)
            query = query.Where(x => x.CreatedAt > parameters.Cursor.Value);

        var items = await query
            .OrderBy(x => x.CreatedAt)
            .ProjectTo<SprintDto>(_mapper.ConfigurationProvider)
            .Take(pageSize + 1)
            .ToListAsync(ct);

        DateTime? nextCursor = null;
        var hasMore = items.Count > pageSize;

        if (hasMore)
        {
            items.RemoveAt(items.Count - 1);
            nextCursor = items[^1].CreatedAt;
        }

        return new PagedList<SprintDto, DateTime?>
        {
            Items = items,
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
            query = query.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(p.Code))
        {
            query = query.Where(x => x.Code == p.Code);
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