using Application.Common;
using Application.Features.Sprints.DTOs;
using Application.Features.WorkItems.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Enums;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ReadModels;

public sealed class SprintsReadModel : ISprintsReadModel
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 50;

    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public SprintsReadModel(
        AppDbContext dbContext,
        IMapper mapper)
    {
        _dbContext = dbContext
            ?? throw new ArgumentNullException(nameof(dbContext));

        _mapper = mapper
            ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<SprintDto?> GetByIdAsync(
        string tenantId,
        string? sprintId,
        string? projectId = null,
        CancellationToken ct = default)
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

        return await query
            .ProjectTo<SprintDto>(
                _mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<SprintWithWorkItemsDto?> GetSprintWithWorkItemsAsync(
     string tenantId,
     string projectId,
     string sprintId,
     CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(projectId) ||
            string.IsNullOrWhiteSpace(sprintId))
        {
            return null;
        }

        var tId = tenantId.Trim();
        var pId = projectId.Trim();
        var sId = sprintId.Trim();

        var sprint = await GetByIdAsync(
            tId,
            sId,
            pId,
            cancellationToken);

        if (sprint is null)
            return null;

        var sprintWorkItems = await _dbContext.WorkItems
            .AsNoTracking()
            .Where(workItem =>
                workItem.Project.TenantId == tId &&
                workItem.ProjectId == pId &&
                workItem.SprintId == sId)
            .OrderBy(workItem => workItem.CreatedAt)
            .ProjectTo<WorkItemListItemDto>(
                _mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var epics = sprintWorkItems
            .Where(workItem =>
                workItem.Type == WorkItemType.Epic)
            .ToList();

        var userStories = sprintWorkItems
            .Where(workItem =>
                workItem.Type == WorkItemType.UserStory)
            .Select(userStory => new SprintUserStoryGroupDto
            {
                UserStory = userStory,

                WorkItems = sprintWorkItems
                    .Where(workItem =>
                        workItem.Type != WorkItemType.Epic &&
                        workItem.Type != WorkItemType.UserStory &&
                        workItem.Parent?.Id == userStory.Id)
                    .ToList()
            })
            .ToList();

        var orphanWorkItems = sprintWorkItems
            .Where(workItem =>
                workItem.Type != WorkItemType.Epic &&
                workItem.Type != WorkItemType.UserStory &&
                workItem.Parent is null)
            .ToList();

        return new SprintWithWorkItemsDto
        {
            Sprint = sprint,
            Epics = epics,
            UserStories = userStories,
            OrphanWorkItems = orphanWorkItems
        };
    }

    public async Task<string?> GetSprintDisplayNameAsync(
        string tenantId,
        string projectId,
        string sprintId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(projectId) ||
            string.IsNullOrWhiteSpace(sprintId))
        {
            return null;
        }

        var tId = tenantId.Trim();
        var pId = projectId.Trim();
        var sId = sprintId.Trim();

        return await _dbContext.Sprints
            .AsNoTracking()
            .Where(sprint =>
                sprint.Id == sId &&
                sprint.ProjectId == pId &&
                sprint.TenantId == tId)
            .Select(sprint =>
                string.IsNullOrEmpty(sprint.Name)
                    ? sprint.Code
                    : sprint.Code + " - " + sprint.Name)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedList<SprintDto, DateTime?>> GetSprintsAsync(
        SprintsSearchAndFilterParametersDto parameters,
        CancellationToken ct = default)
    {
        parameters ??=
            new SprintsSearchAndFilterParametersDto();

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

        var items = await query
            .OrderBy(sprint => sprint.CreatedAt)
            .ProjectTo<SprintDto>(
                _mapper.ConfigurationProvider)
            .Take(pageSize + 1)
            .ToListAsync(ct);

        DateTime? nextCursor = null;

        if (items.Count > pageSize)
        {
            items.RemoveAt(items.Count - 1);

            nextCursor = items.Count > 0
                ? items[^1].CreatedAt
                : null;
        }

        return new PagedList<SprintDto, DateTime?>
        {
            Items = items,
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
                sprint.StartDate.HasValue &&
                sprint.StartDate.Value >= parameters.StartAfter.Value);
        }

        if (parameters.StartBefore.HasValue)
        {
            query = query.Where(sprint =>
                sprint.StartDate.HasValue &&
                sprint.StartDate.Value <= parameters.StartBefore.Value);
        }

        if (parameters.EndAfter.HasValue)
        {
            query = query.Where(sprint =>
                sprint.EndDate.HasValue &&
                sprint.EndDate.Value >= parameters.EndAfter.Value);
        }

        if (parameters.EndBefore.HasValue)
        {
            query = query.Where(sprint =>
                sprint.EndDate.HasValue &&
                sprint.EndDate.Value <= parameters.EndBefore.Value);
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