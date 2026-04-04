using Application.Common;
using Application.Features.Projects.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Models;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ReadModels;

public sealed class ProjectsReadModel : IProjectsReadModel
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public ProjectsReadModel(AppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public Task<ProjectDto?> GetByIdAsync(string tenantId, string projectId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(projectId))
            return Task.FromResult<ProjectDto?>(null);

        var tId = tenantId.Trim();
        var pId = projectId.Trim();

        return _dbContext.Projects
            .AsNoTracking()
            .Where(p => p.TenantId == tId && p.Id == pId)
            .ProjectTo<ProjectDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<WorkItemSnapshot>> GetProjectWorkItemSnapshotAsync(string tenantId, string projectId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(projectId))
            return new List<WorkItemSnapshot>();

        var tId = tenantId.Trim();
        var pId = projectId.Trim();

        return await _dbContext.WorkItems
            .AsNoTracking()
            .Where(wi => wi.Project.TenantId == tId && wi.Project.Id == pId)
            .Select(wi => new WorkItemSnapshot(
                wi.Id,
                wi.Title,
                wi.Type,
                wi.Status,
                null))
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedList<ProjectDto, DateTime?>> GetUserProjectsAsync(
        string userId,
        ProjectsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default)
    {
        var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

        var query = _dbContext.Projects
            .Where(p => p.CreatedByUserId == userId ||
            (
                p.TeamId != null &&
                _dbContext.TeamMembers.Any(tm => tm.TeamId == p.TeamId && tm.UserId == userId)
            ))
            .AsNoTracking();

        query = ApplyFilters(query, parameters);

        if (parameters.Cursor.HasValue)
            query = query.Where(p => p.CreatedAt <= parameters.Cursor.Value);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .ProjectTo<ProjectDto>(_mapper.ConfigurationProvider)
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;
        if (items.Count > pageSize)
        {
            nextCursor = items[^1].CreatedAt;
            items.RemoveAt(items.Count - 1);
        }

        return new PagedList<ProjectDto, DateTime?>
        {
            Items = items,
            NextCursor = nextCursor
        };
    }

    public async Task<PagedList<ProjectDto, DateTime?>> GetProjectsAsync(
        ProjectsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default)
    {
        var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

        var query = _dbContext.Projects.AsNoTracking();

        query = ApplyFilters(query, parameters);

        if (parameters.Cursor.HasValue)
        {
            query = query.Where(p => p.CreatedAt < parameters.Cursor.Value);
        }

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .ProjectTo<ProjectDto>(_mapper.ConfigurationProvider)
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;
        var hasMore = items.Count > pageSize;

        if (hasMore)
        {
            items.RemoveAt(items.Count - 1); 
            nextCursor = items[^1].CreatedAt; 
        }

        return new PagedList<ProjectDto, DateTime?>
        {
            Items = items,
            NextCursor = nextCursor
        };
    }

    private static IQueryable<ProjectEntity> ApplyFilters(
        IQueryable<ProjectEntity> query,
        ProjectsSearchAndFilterParametersDto p)
    {
        if (!string.IsNullOrWhiteSpace(p.TeamId))
        {
            var teamId = p.TeamId.Trim();
            query = query.Where(x => x.TeamId == teamId);
        }

        if (p.AssignedToTeam)
            query = query.Where(x => x.TeamId != null);

        if (!string.IsNullOrWhiteSpace(p.NameContains))
        {
            var term = p.NameContains.Trim();
            query = query.Where(x => x.Name.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(p.Key))
            query = query.Where(x => x.Key == p.Key);

        if (p.Status.HasValue)
            query = query.Where(x => x.Status == p.Status.Value);

        if (p.StartAfter.HasValue)
            query = query.Where(x => x.StartDate != null && x.StartDate >= p.StartAfter.Value);

        if (p.StartBefore.HasValue)
            query = query.Where(x => x.StartDate != null && x.StartDate <= p.StartBefore.Value);

        if (p.EndAfter.HasValue)
            query = query.Where(x => x.EndDate != null && x.EndDate >= p.EndAfter.Value);

        if (p.EndBefore.HasValue)
            query = query.Where(x => x.EndDate != null && x.EndDate <= p.EndBefore.Value);

        if (p.CreatedAfter.HasValue)
            query = query.Where(x => x.CreatedAt >= p.CreatedAfter.Value);

        if (p.CreatedBefore.HasValue)
            query = query.Where(x => x.CreatedAt <= p.CreatedBefore.Value);

        return query;
    }
}