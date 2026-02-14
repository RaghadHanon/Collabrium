using Application.Common;
using Application.Features.WorkItems.DTOs;
using Application.Features.WorkItems.DTOs.Relations;
using Application.Interfaces.ReadModels;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Core;
using Domain.Enums;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.Mappers;
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
            .Include(w => w.Relations)
            .AsNoTracking()
            .Where(w => w.Project.TenantId == tId && w.ProjectId == pId && w.Id == wId)
            .FirstOrDefaultAsync(cancellationToken);

        var allowedStatuses = GetAllowedStatuses(entity);
        var parentRelation = GetParentAsync(entity);
        var dto = entity is null ? null : _mapper.Map<WorkItemDto>(entity);

        if (dto != null)
        {
            dto.AllowedStatuses = allowedStatuses;
            dto.Parent = _mapper.Map<WorkItemRelationDto>(parentRelation);
        }

        return entity is null ? null : dto;
    }

    private WorkItemRelationEntity? GetParentAsync(WorkItemEntity? entity)
    {
        return entity is null
            ? null
            : entity.Relations
                .Where(r => r.RelationType == WorkItemRelationType.Parent)
                .FirstOrDefault();
    }

    private static List<WorkItemStatus> GetAllowedStatuses(WorkItemEntity? entity)
    {
        if (entity is null)
            return new List<WorkItemStatus>();

        var domainItem = WorkItemMapper.ToDomain(entity!);
        return domainItem != null
            ? domainItem.GetAllowedStatusTransitions()
            : new List<WorkItemStatus>();
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

        var items = await _dbContext.WorkItems
            .Where(w => w.Type != WorkItemType.UserStory && w.Type != WorkItemType.Epic && w.SprintId != null)
            .OrderBy(w => w.CreatedAt)
            .Select(w => new WorkItemListItemDto
            {
                Id = w.Id,
                Title = w.Title,
                Type = w.Type,
                Status = w.Status,
                Priority = w.Priority,
                SprintId = w.SprintId,
                ProjectId = w.ProjectId,
                IsBlocked = w.IsBlocked,
                AssignedToUserId = w.AssignedToUserId,
                CreatedAt = w.CreatedAt,

                Parent = w.Relations
                    .Where(r => r.RelationType == WorkItemRelationType.Parent)
                    .Select(r => new WorkItemRelationDto
                    {
                        RelationType = r.RelationType,
                        TargetWorkItem = new RelationParticipantWorkItemDto
                        {
                            Id = r.TargetWorkItem.Id,
                            Title = r.TargetWorkItem.Title,
                            Type = r.TargetWorkItem.Type,
                            Status = r.TargetWorkItem.Status,
                        }
                    }
                    )
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return items;
    }

    public List<WorkItemStatus> GetAllowedStatusesByType(WorkItemType type)
    {
        return WorkItemStatusRules.GetAllowedStatusesByType(type) ?? new List<WorkItemStatus>();
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
            .Where(x => x.Type == type).Select(w => new WorkItemListItemDto
            {
                Id = w.Id,
                Title = w.Title,
                Type = w.Type,
                Status = w.Status,
                Priority = w.Priority,
                SprintId = w.SprintId,
                ProjectId = w.ProjectId,
                IsBlocked = w.IsBlocked,
                AssignedToUserId = w.AssignedToUserId,
                CreatedAt = w.CreatedAt,

                Parent = w.Relations
                    .Where(r => r.RelationType == WorkItemRelationType.Parent)
                    .Select(r => new WorkItemRelationDto
                    {
                        RelationType = r.RelationType,
                        TargetWorkItem = new RelationParticipantWorkItemDto
                        {
                            Id = r.TargetWorkItem.Id,
                            Title = r.TargetWorkItem.Title,
                            Type = r.TargetWorkItem.Type,
                            Status = r.TargetWorkItem.Status,
                        }
                    }
                    )
                    .FirstOrDefault()
            })
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
            .OrderByDescending(x => x.CreatedAt).Select(w => new WorkItemListItemDto
            {
                Id = w.Id,
                Title = w.Title,
                Type = w.Type,
                Status = w.Status,
                Priority = w.Priority,
                SprintId = w.SprintId,
                ProjectId = w.ProjectId,
                IsBlocked = w.IsBlocked,
                AssignedToUserId = w.AssignedToUserId,
                CreatedAt = w.CreatedAt,

                Parent = w.Relations
                    .Where(r => r.RelationType == WorkItemRelationType.Parent)
                    .Select(r => new WorkItemRelationDto
                    {
                        RelationType = r.RelationType,
                        TargetWorkItem = new RelationParticipantWorkItemDto
                        {
                            Id = r.TargetWorkItem.Id,
                            Title = r.TargetWorkItem.Title,
                            Type = r.TargetWorkItem.Type,
                            Status = r.TargetWorkItem.Status,
                        }
                    }
                    )
                    .FirstOrDefault()
            })
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

    // relations
    public async Task<WorkItemRelationDto?> GetRelationByIdAsync(
       string projectId,
       string sourceWorkItemId,
       string relationId,
       CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(projectId) ||
            string.IsNullOrWhiteSpace(sourceWorkItemId) ||
            string.IsNullOrWhiteSpace(relationId))
            return null;

        var pId = projectId.Trim();
        var swId = sourceWorkItemId.Trim();
        var rId = relationId.Trim();

        return await _dbContext.WorkItemRelations
            .AsNoTracking()
            .Where(r =>
                r.Id == rId &&
                r.SourceWorkItemId == swId &&
                r.SourceWorkItem.ProjectId == pId)
            .ProjectTo<WorkItemRelationDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<WorkItemRelationDto>?> GetRelationsByTypeAsync(
        string tenantId,
        string projectId,
        string workItemId,
        WorkItemRelationType? relationType = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(workItemId))
            return null;

        var tId = tenantId.Trim();
        var wId = workItemId.Trim();

        var query = _dbContext.WorkItemRelations
            .AsNoTracking()
            .Where(r =>
                r.SourceWorkItem.Project.TenantId == tId &&
                r.SourceWorkItem.ProjectId == projectId &&
                r.SourceWorkItemId == wId);

        if (relationType.HasValue)
            query = query.Where(r => r.RelationType == relationType.Value);

        return await query
            .ProjectTo<WorkItemRelationDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}