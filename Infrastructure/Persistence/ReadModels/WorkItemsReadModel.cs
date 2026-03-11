using Application.Common;
using Application.Features.Projects.DTOs.Boards;
using Application.Features.WorkItems.DTOs;
using Application.Features.WorkItems.DTOs.Attachment;
using Application.Features.WorkItems.DTOs.Comments;
using Application.Features.WorkItems.DTOs.Relations;
using Application.Interfaces.ReadModels;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Core;
using Domain.Enums;
using Domain.Models;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Infrastructure.Persistence.ReadModels;

public class WorkItemsReadModel : IWorkItemsReadModel
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private static readonly List<WorkItemStatus> _boardStatuses = Enum.GetValues<WorkItemStatus>().ToList();

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
            .Include(w => w.Parent)
            .Include(w => w.CodeLinks)
            .ThenInclude(cl => cl.CreatedByUser)
            .AsNoTracking()
            .Where(w => w.Project.TenantId == tId && w.ProjectId == pId && w.Id == wId)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
            return null;

        var allowedStatuses = GetAllowedStatusTransitions(entity);
        var dto = entity is null ? null : _mapper.Map<WorkItemDto>(entity);

        if (dto != null)
            dto.AllowedStatuses = allowedStatuses;

        return entity is null ? null : dto;
    }

    private async Task<RelationParticipantWorkItemDto?> GetParentAsync(string tennatId, string projectId, string? parentId, CancellationToken cancellationToken)
    {
        return parentId is null
            ? null
            : await _dbContext.WorkItems
                    .AsNoTracking()
                    .Where(w => w.Project.TenantId == tennatId && w.ProjectId == projectId && w.Id == parentId)
                    .ProjectTo<RelationParticipantWorkItemDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<WorkItemSnapshot>> GetChildrenAsync(
        string tenantId,
        string projectId,
        string workItemId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(projectId) ||
            string.IsNullOrWhiteSpace(workItemId))
            return new List<WorkItemSnapshot>();

        var tId = tenantId.Trim();
        var pId = projectId.Trim();
        var wId = workItemId.Trim();

        return await _dbContext.WorkItemRelations
            .AsNoTracking()
            .Where(r =>
                r.RelationType == WorkItemRelationType.Child &&
                r.SourceWorkItemId == wId &&
                r.SourceWorkItem.ProjectId == pId &&
                r.SourceWorkItem.Project.TenantId == tId)
            .Select(r => new WorkItemSnapshot(
                r.TargetWorkItem.Id,
                r.TargetWorkItem.Title,
                r.TargetWorkItem.Type,
                r.TargetWorkItem.Status,
                null))
            .ToListAsync(cancellationToken);
    }

    private static List<WorkItemStatus> GetAllowedStatusTransitions(WorkItemEntity? entity)
    {
        if (entity is null)
            return new List<WorkItemStatus>();

        var domainItem = WorkItemMapper.ToDomain(entity!);
        return domainItem != null
            ? domainItem.GetAllowedStatusTransitions()
            : new List<WorkItemStatus>();
    }

    public async Task<BoardDto> GetBoardItemsAsync(
       WorkItemsSearchAndFilterParametersDto parameters,
       CancellationToken cancellationToken = default)
    {
        var allItems = await GetBoardItemsQueryResultAsync(
                   parameters,
                   cancellationToken);

        var userStories = allItems
            .Where(x => x.Type == WorkItemType.UserStory)
            .ToList();

        var nonUserStoryItemsWithParents = allItems
            .Where(x => x.Type != WorkItemType.UserStory && x.Parent != null)
            .ToList();

        var orphanNonUserStoryItems = allItems
            .Where(x => x.Type != WorkItemType.UserStory && x.Parent == null)
            .ToList();

        var itemsByParent = nonUserStoryItemsWithParents
            .GroupBy(i => i.Parent!.Id)
            .ToDictionary(g => g.Key, g => g.ToList());

        return new BoardDto
        {
            ProjectId = parameters.ProjectId!,
            SprintId = parameters.SprintId,
            UserStoryRows = BuildUserStoryRows(userStories, itemsByParent),
            OrphanRow = BuildOrphanRow(orphanNonUserStoryItems),
        };
    }

    private async Task<List<WorkItemListItemDto>> GetBoardItemsQueryResultAsync(
        WorkItemsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.WorkItems
            .Include(i => i.Project)
            .Include(i => i.Sprint)
            .AsNoTracking();

        var typeFilterApplied = parameters.Type;
        if (parameters.Type.HasValue)
        {
            query = query.Where(i => i.Type == typeFilterApplied || i.Type == WorkItemType.UserStory);
            parameters.Type = null;
        }

        query = ApplyFilters(query, parameters);

        var items = await query
            .Where(w =>
                w.Type != WorkItemType.Epic &&
                w.Sprint != null &&
                w.Sprint.Status == SprintStatus.Active)
            .OrderBy(w => w.CreatedAt)
            .Select(w => new WorkItemListItemDto
            {
                Id = w.Id,
                Title = w.Title,
                Code = w.Code,
                Type = w.Type,
                Status = w.Status,
                Priority = w.Priority,
                SprintId = w.SprintId,
                ProjectId = w.ProjectId,
                IsBlocked = w.IsBlocked,
                AssignedToUserId = w.AssignedToUserId,
                CreatedAt = w.CreatedAt,

                AllowedStatuses = GetAllowedStatusTransitions(w).ToList(),

                Parent = _dbContext.WorkItems
                    .Where(r => r.Id == w.ParentId)
                    .Select(r => new RelationParticipantWorkItemDto
                    {
                        Id = r.Id,
                        Code = r.Code,
                        Title = r.Title,
                        Type = r.Type,
                        Status = r.Status,

                    })
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return items;
    }

    private List<UserStoryBoardGroupDto> BuildUserStoryRows(
        List<WorkItemListItemDto> userStories,
        Dictionary<string, List<WorkItemListItemDto>> itemsByParent)
    {
        var rows = new List<UserStoryBoardGroupDto>();

        foreach (var us in userStories)
        {
            itemsByParent.TryGetValue(us.Id, out var children);
            children ??= new List<WorkItemListItemDto>();

            var columns = children.Count != 0 ? _boardStatuses
                .Select(status =>
                {
                    var items = children
                        .Where(i => i.Status == status)
                        .ToList();

                    return new BoardColumnDto
                    {
                        Status = status,
                        Title = Regex.Replace(status.ToString(), "(\\B[A-Z])", " $1"),
                        Count = items.Count,
                        Items = items
                    };
                })
                .ToList() : null;

            rows.Add(new UserStoryBoardGroupDto
            {
                ParentUserStory = us,
                Columns = columns
            });
        }

        return rows;
    }

    private static OrphanBoardGroupDto BuildOrphanRow(
        List<WorkItemListItemDto> items)
    {

        return new OrphanBoardGroupDto
        {
            Columns = items.Count != 0 ? _boardStatuses.Select(status => new BoardColumnDto
            {
                Status = status,
                Title = Regex.Replace(status.ToString(), "(\\B[A-Z])", " $1"),
                Items = items
                    .Where(i => string.IsNullOrEmpty(i.Parent?.Id) && i.Status == status)
                    .ToList()
            }).ToList() : null
        };
    }

    public List<WorkItemStatus> GetWorkItemWorkflowByType(WorkItemType type)
    {
        return WorkItemStatusRules.GetWorkItemWorkflowByType(type) ?? new List<WorkItemStatus>();
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
            .Select(w => new WorkItemListItemDto
            {
                Id = w.Id,
                Title = w.Title,
                Code = w.Code,
                Type = w.Type,
                Status = w.Status,
                Priority = w.Priority,
                SprintId = w.SprintId,
                ProjectId = w.ProjectId,
                IsBlocked = w.IsBlocked,
                AssignedToUserId = w.AssignedToUserId,
                CreatedAt = w.CreatedAt,

                AllowedStatuses = GetAllowedStatusTransitions(w).ToList(),

                Parent = _dbContext.WorkItems
                    .Where(r => r.Id == w.ParentId)
                    .Select(r => new RelationParticipantWorkItemDto
                    {
                        Id = r.Id,
                        Code = r.Code,
                        Title = r.Title,
                        Type = r.Type,
                        Status = r.Status,

                    })
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
            query = query.Where(x => x.CreatedAt <= parameters.Cursor.Value);

        var items = await query
            .OrderByDescending(x => x.CreatedAt).Select(w => new WorkItemListItemDto
            {
                Id = w.Id,
                Title = w.Title,
                Code = w.Code,
                Type = w.Type,
                Status = w.Status,
                Priority = w.Priority,
                SprintId = w.SprintId,
                ProjectId = w.ProjectId,
                IsBlocked = w.IsBlocked,
                AssignedToUserId = w.AssignedToUserId,
                CreatedAt = w.CreatedAt,

                AllowedStatuses = GetAllowedStatusTransitions(w).ToList(),

                Parent = _dbContext.WorkItems
                    .Where(r => r.Id == w.ParentId)
                    .Select(r => new RelationParticipantWorkItemDto
                    {
                        Id = r.Id,
                        Code = r.Code,
                        Title = r.Title,
                        Type = r.Type,
                        Status = r.Status,

                    })
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

        if (!string.IsNullOrWhiteSpace(parameters.Code))
        {
            var code = parameters.Code.Trim();
            query = query.Where(x => x.Code == code);
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

        if (!string.IsNullOrWhiteSpace(parameters.CreatedByUserId))
        {
            var aId = parameters.CreatedByUserId.Trim();
            query = query.Where(x => x.CreatedByUserId == aId);
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

    public async Task<List<RelationParticipantWorkItemDto>?> GetRelationCandidates(
    string tenantId,
    string projectId,
    string sourceWorkItemId,
    WorkItemRelationType relationType,
    CancellationToken cancellationToken = default)
    {
        tenantId = tenantId.Trim();
        projectId = projectId.Trim();
        sourceWorkItemId = sourceWorkItemId.Trim();

        var sourceWorkItem = await _dbContext.WorkItems
            .AsNoTracking()
            .Where(w =>
                w.Project.TenantId == tenantId &&
                w.ProjectId == projectId &&
                w.Id == sourceWorkItemId)
            .Select(w => new { w.Id, w.SprintId, w.Type, w.ParentId })
            .FirstOrDefaultAsync(cancellationToken);

        if (sourceWorkItem == null)
            return null;

        var query = _dbContext.WorkItems
            .AsNoTracking()
            .Where(w =>
                w.Project.TenantId == tenantId &&
                w.ProjectId == projectId &&
                w.Id != sourceWorkItem.Id && 
                (w.SprintId == null || w.SprintId == sourceWorkItem.SprintId));

        var allowedTypes = GetAllowedTargetTypes(sourceWorkItem.Type, relationType);

        if (allowedTypes.Any())
            query = query.Where(w => allowedTypes.Contains(w.Type));

        if (relationType == WorkItemRelationType.Child)
            query = query.Where(w => w.ParentId == null);

        return await query
            .ProjectTo<RelationParticipantWorkItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    private static IReadOnlyList<WorkItemType> GetAllowedTargetTypes(
    WorkItemType source,
    WorkItemRelationType relationType)
    {
        if (relationType == WorkItemRelationType.RelatesTo)
        {
            return Enum.GetValues<WorkItemType>();
        }

        return (source, relationType) switch
        {
            (WorkItemType.UserStory, WorkItemRelationType.Parent)
                => new[] { WorkItemType.Epic },

            (WorkItemType.TaskItem, WorkItemRelationType.Parent)
                => new[] { WorkItemType.UserStory },

            (WorkItemType.Bug, WorkItemRelationType.Parent)
                => new[] { WorkItemType.UserStory },

            (WorkItemType.TestCase, WorkItemRelationType.Parent)
                => new[] { WorkItemType.UserStory },

            (WorkItemType.Epic, WorkItemRelationType.Child)
                => new[] { WorkItemType.UserStory },

            (WorkItemType.UserStory, WorkItemRelationType.Child)
                => new[]
                {
                WorkItemType.TaskItem,
                WorkItemType.Bug,
                WorkItemType.TestCase
                },

            (WorkItemType.Bug, WorkItemRelationType.FoundByTestCase)
                => new[] { WorkItemType.TestCase },

            _ => Array.Empty<WorkItemType>()
        };
    }

    // comments
    public async Task<PagedList<CommentDto, DateTime?>> GetWorkItemComments(
        CommentsQueryParametersDto parameters,
        string workItemId,
        CancellationToken cancellationToken = default)
    {
        var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

        var wId = workItemId.Trim();
        var query = _dbContext.Comments
            .AsNoTracking()
            .AsQueryable();

        if (parameters.Cursor.HasValue)
            query = query.Where(x => x.CreatedAt <= parameters.Cursor.Value);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Where(c => c.WorkItemId == wId)
            .ProjectTo<CommentDto>(_mapper.ConfigurationProvider)
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;
        if (items.Count > pageSize)
        {
            nextCursor = items[^1].CreatedAt;
            items.RemoveAt(items.Count - 1);
        }

        return new PagedList<CommentDto, DateTime?>
        {
            Items = items,
            NextCursor = nextCursor
        };
    }

    // attachments
    public async Task<List<AttachmentDto>> GetWorkItemAttachments(
        string workItemId,
        CancellationToken cancellationToken = default)
    {
        var wId = workItemId.Trim();

        return await _dbContext.Attachments
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Where(c => c.WorkItemId == wId)
            .ProjectTo<AttachmentDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}