using Application.Common;
using Application.Features.WorkItems.DTOs;
using Application.Interfaces.Repositories;
using Domain.Enums;
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

    public async Task<bool> IsExistAsync(string tenantId, string projectId, string workItemId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(workItemId))
            return false;

        var tId = tenantId.Trim();
        var wId = workItemId.Trim();

        return await _dbContext.WorkItems
            .AnyAsync(w => w.Project.TenantId == tId && w.Id == wId && w.ProjectId == projectId, cancellationToken);
    }

    public async Task MoveIncompleteWorkItemsFromSprintToBacklogAsync(string tenantId, string sprintId, string projectId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(sprintId) || string.IsNullOrWhiteSpace(projectId))
            return;

        var sId = sprintId.Trim();

        var list = await _dbContext.WorkItems
                .Where(w => w.SprintId == sId && tenantId == w.Project.TenantId && w.ProjectId == projectId && w.Status != WorkItemStatus.Done)
                .ToListAsync(cancellationToken);

        foreach (var workItem in list)
        {
            workItem.SprintId = null;
            workItem.Status = WorkItemStatus.ToDo;
        }

        return;
    }

    public async Task<int> UnassignAllProjectWorkItemsAsync(
        string tenantId,
        string projectId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(projectId))
        {
            return 0;
        }

        var tId = tenantId.Trim();
        var pId = projectId.Trim();
        var updatedAt = DateTime.UtcNow;

        return await _dbContext.WorkItems
            .Where(workItem =>
                workItem.ProjectId == pId &&
                workItem.Project.TenantId == tId &&
                workItem.AssignedToUserId != null &&
                workItem.Status != WorkItemStatus.Done)
            .ExecuteUpdateAsync(
                setters => 
                setters.SetProperty( workItem => workItem.AssignedToUserId, (string?) null)
                    .SetProperty(workItem => workItem.UpdatedAt, updatedAt),cancellationToken);
    }

    public async Task<List<WorkItem>> GetWorkItemsListFromIds(string tenantId, string projectId, string sprintId,
        string[] workItemIds, CancellationToken cancellationToken)
    {
        var workItemsEntity = await _dbContext.WorkItems
            .Where(w => workItemIds.Contains(w.Id) && w.ProjectId == projectId && w.Project.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var workItems = workItemsEntity
            .Select(e => WorkItemMapper.ToDomain(e, includeChildren: false))
            .ToList();

        return workItems;
    }



    public async Task<PagedList<WorkItem, DateTime?>> GetPagedAsync(
        WorkItemsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default)
    {
        var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

        var query = _dbContext.WorkItems.AsNoTracking();

        query = ApplyFilters(query, parameters);

        if (parameters.Cursor.HasValue)
            query = query.Where(x => x.CreatedAt < parameters.Cursor.Value);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;
        var hasMore = items.Count > pageSize;

        if (hasMore)
        {
            items.RemoveAt(items.Count - 1);
            nextCursor = items[^1].CreatedAt;
        }

        return new PagedList<WorkItem, DateTime?>
        {
            Items = items.Select(e => WorkItemMapper.ToDomain(e, includeChildren: false)).ToList(),
            NextCursor = nextCursor
        };
    }

    private static IQueryable<WorkItemEntity> ApplyFilters(IQueryable<WorkItemEntity> query, WorkItemsSearchAndFilterParametersDto parameters)
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

        if (!string.IsNullOrWhiteSpace(parameters.TitleContains))
        {
            var term = parameters.TitleContains.Trim();
            query = query.Where(x => x.Title.Contains(term));
        }

        if (parameters.CreatedAfter.HasValue)
            query = query.Where(x => x.CreatedAt >= parameters.CreatedAfter.Value);

        if (parameters.CreatedBefore.HasValue)
            query = query.Where(x => x.CreatedAt <= parameters.CreatedBefore.Value);

        if (parameters.IsAssigned.HasValue)
        {
            query = parameters.IsAssigned.Value ? query.Where(x => x.AssignedToUserId != null) : query.Where(x => x.AssignedToUserId == null);
        }

        if (!string.IsNullOrWhiteSpace(parameters.ParentId))
        {
            var pId = parameters.ParentId.Trim();
            query = query.Where(x => x.ParentId == pId);
        }

        return query;
    }

    // relations
    public async Task PersistRelationAsync(
        string tenantId,
        string projectId,
        string sourceWorkItemId,
        string targetWorkItemId,
        WorkItemRelationType relationType,
        CancellationToken ct = default)
    {
        var sourceWorkItemEntity = await _dbContext.WorkItems
            .Include(w => w.Relations)
            .FirstOrDefaultAsync(w =>
            w.Project.TenantId == tenantId && w.Id == sourceWorkItemId && w.ProjectId == projectId, ct);

        var targetWorkItemEntity = await _dbContext.WorkItems
            .Include(w => w.Relations)
            .FirstOrDefaultAsync(w =>
            w.Project.TenantId == tenantId && w.Id == targetWorkItemId && w.ProjectId == projectId, ct);

        if (sourceWorkItemEntity is null || targetWorkItemEntity is null)
            return;

        var sourceWorkItemDomain = WorkItemMapper.ToDomain(sourceWorkItemEntity, true);
        var targetWorkItemDomain = WorkItemMapper.ToDomain(targetWorkItemEntity, true);

        sourceWorkItemDomain.AddRelation(targetWorkItemDomain, relationType);
        sourceWorkItemEntity.Relations.Add(CreateRelationRow(sourceWorkItemId, targetWorkItemId, relationType));

        EnsureInverse(targetWorkItemDomain, sourceWorkItemDomain, targetWorkItemEntity, sourceWorkItemEntity, relationType);
        WorkItemMapper.UpdateEntity(sourceWorkItemDomain, sourceWorkItemEntity);
    }

    private void EnsureInverse(
        WorkItem targetWorkItemDomain,
        WorkItem sourceWorkItemDomain,
        WorkItemEntity targetWorkItemEntity,
        WorkItemEntity sourceWorkItemEntity,
        WorkItemRelationType relationType)
    {
        if (relationType == WorkItemRelationType.Parent)
        {
            targetWorkItemDomain.AddRelation(sourceWorkItemDomain, WorkItemRelationType.Child);
            targetWorkItemEntity.Relations
                .Add(CreateRelationRow(targetWorkItemEntity.Id, sourceWorkItemEntity.Id, WorkItemRelationType.Child));
        }
        else if (relationType == WorkItemRelationType.Child)
        {
            targetWorkItemDomain.AddRelation(sourceWorkItemDomain, WorkItemRelationType.Parent);
            targetWorkItemEntity.Relations
                .Add(CreateRelationRow(targetWorkItemEntity.Id, sourceWorkItemEntity.Id, WorkItemRelationType.Parent));
        }

        WorkItemMapper.UpdateEntity(targetWorkItemDomain, targetWorkItemEntity);
    }

    private static WorkItemRelationEntity CreateRelationRow(
        string sourceWorkItemId,
        string targetWorkItemId,
        WorkItemRelationType relationType)
    {
        return new WorkItemRelationEntity
        {
            SourceWorkItemId = sourceWorkItemId,
            TargetWorkItemId = targetWorkItemId,
            RelationType = relationType
        };
    }

    public async Task<bool> DeleteRelationAsync(
        string tenantId,
        string projectId,
        string sourceWorkItemId,
        string relationId,
        CancellationToken ct = default)
    {
        tenantId = tenantId.Trim();
        projectId = projectId.Trim();
        relationId = relationId.Trim();

        var relationEntity = await _dbContext.WorkItemRelations
            .Include(r => r.SourceWorkItem)
                .ThenInclude(w => w.Project)
            .Include(r => r.SourceWorkItem)
                .ThenInclude(w => w.Relations)
            .FirstOrDefaultAsync(r =>
                r.Id == relationId &&
                r.SourceWorkItem.ProjectId == projectId &&
                r.SourceWorkItem.Project.TenantId == tenantId &&
                r.SourceWorkItemId == sourceWorkItemId
                , ct);

        if (relationEntity is null) return false;

        var sourceDomain = WorkItemMapper.ToDomain(relationEntity.SourceWorkItem, includeChildren: true);
        sourceDomain.RemoveRelation(relationEntity.TargetWorkItemId, relationEntity.RelationType);
        _dbContext.WorkItemRelations.Remove(relationEntity);

        await RemoveInverseRelation(relationEntity, ct);

        WorkItemMapper.UpdateEntity(sourceDomain, relationEntity.SourceWorkItem);

        return true;
    }

    private async Task RemoveInverseRelation(WorkItemRelationEntity relation, CancellationToken ct)
    {
        if (relation.RelationType is WorkItemRelationType.Parent or WorkItemRelationType.Child)
        {
            var inverseType = relation.RelationType == WorkItemRelationType.Parent
                ? WorkItemRelationType.Child
                : WorkItemRelationType.Parent;

            var inverse = await _dbContext.WorkItemRelations
                .FirstOrDefaultAsync(r =>
                    r.SourceWorkItemId == relation.TargetWorkItemId &&
                    r.TargetWorkItemId == relation.SourceWorkItemId &&
                    r.RelationType == inverseType, ct);

            if (inverse is not null)
                _dbContext.WorkItemRelations.Remove(inverse);
        }
    }

    public async Task<bool> HasRelationsAsync(
        string tenantId,
        string projectId,
        string workItemId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(projectId) ||
            string.IsNullOrWhiteSpace(workItemId))
        {
            return false;
        }

        var tId = tenantId.Trim();
        var pId = projectId.Trim();
        var wId = workItemId.Trim();

        return await _dbContext.WorkItemRelations
            .AsNoTracking()
            .AnyAsync(
                relation =>
                    (relation.SourceWorkItemId == wId ||
                     relation.TargetWorkItemId == wId) &&
                    relation.SourceWorkItem.ProjectId == pId &&
                    relation.SourceWorkItem.Project.TenantId == tId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<WorkItem>> GetChildrenByParentIdAsync(
    string tenantId,
    string projectId,
    string parentId,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(projectId) ||
            string.IsNullOrWhiteSpace(parentId))
        {
            return [];
        }

        var normalizedTenantId = tenantId.Trim();
        var normalizedProjectId = projectId.Trim();
        var normalizedParentId = parentId.Trim();

        var entities = await _dbContext.WorkItems
            .AsNoTracking()
            .Include(workItem => workItem.Project)
            .Include(workItem => workItem.Comments)
            .Include(workItem => workItem.Relations)
            .Include(workItem => workItem.CodeLinks)
            .Include(workItem => workItem.Attachments)
            .Where(workItem =>
                workItem.Project.TenantId == normalizedTenantId &&
                workItem.ProjectId == normalizedProjectId &&
                workItem.ParentId == normalizedParentId)
            .ToListAsync(cancellationToken);

        return entities
            .Select(entity =>
                WorkItemMapper.ToDomain(
                    entity,
                    includeChildren: true))
            .ToList();
    }

    // comments
    public async Task AddCommentOnWorkItemAsync(
        string tenantId,
        string projectId,
        string workItemId,
        WorkItemComment comment,
        CancellationToken cancellationToken = default)
    {
        tenantId = tenantId.Trim();
        projectId = projectId.Trim();
        workItemId = workItemId.Trim();

        var workItemEntity = await _dbContext.WorkItems
            .Include(r => r.Comments)
            .FirstOrDefaultAsync(
            r => r.ProjectId == projectId &&
                 r.Project.TenantId == tenantId &&
                 r.Id == workItemId
            );
        if (workItemEntity == null)
            return;

        var commentEntity = WorkItemCommentMapper.ToEntity(comment, workItemId);
        workItemEntity.Comments.Add(commentEntity);
    }

    public async Task UpdateCommentOfWorkItemAsync(
        string workItemId,
        WorkItemComment comment,
        CancellationToken cancellationToken = default)
    {
        workItemId = workItemId.Trim();

        var commentEntity = await _dbContext.WorkItemComments
            .Include(c => c.WorkItem)
            .FirstOrDefaultAsync(
                c => c.Id == comment.Id &&
                     c.WorkItemId == workItemId
            );

        if (commentEntity == null)
            return;

        WorkItemCommentMapper.UpdateEntity(comment, commentEntity);
    }

    public async Task RemoveCommentFromWorkItemAsync(
        string workItemId,
        string commentId,
        CancellationToken cancellationToken = default)
    {
        commentId = commentId.Trim();
        workItemId = workItemId.Trim();

        var commentEntity = await _dbContext.WorkItemComments
           .Include(c => c.WorkItem)
           .FirstOrDefaultAsync(
               c => c.Id == commentId &&
                    c.WorkItemId == workItemId
           );

        if (commentEntity == null)
            return;

        _dbContext.Comments.Remove(commentEntity);
    }

    public async Task<WorkItemComment?> GetCommentOnWorkItemByIdAsync(
        string workItemId,
        string commentId,
        CancellationToken cancellationToken = default)
    {
        commentId = commentId.Trim();
        workItemId = workItemId.Trim();

        var commentEntity = await _dbContext.WorkItemComments
            .FirstOrDefaultAsync(
            r => r.Id == commentId &&
                 r.WorkItemId == workItemId
            );
        return commentEntity == null ? null : WorkItemCommentMapper.ToDomain(commentEntity);
    }

    // codeLinkes
    public async Task AddCodeLinkOnWorkItemAsync(
        string tenantId,
        string projectId,
        string workItemId,
        CodeLink codeLink,
        CancellationToken cancellationToken = default)
    {
        tenantId = tenantId.Trim();
        projectId = projectId.Trim();
        workItemId = workItemId.Trim();

        var workItemEntity = await _dbContext.WorkItems
            .Include(r => r.CodeLinks)
            .FirstOrDefaultAsync(
            r => r.ProjectId == projectId &&
                 r.Project.TenantId == tenantId &&
                 r.Id == workItemId
            );
        if (workItemEntity == null)
            return;

        var codeLinkEntity = CodeLinkMapper.ToEntity(codeLink, workItemId);
        workItemEntity.CodeLinks.Add(codeLinkEntity);
    }

    public async Task UpdateCodeLinkOfWorkItemAsync(
        string workItemId,
        CodeLink codeLink,
        CancellationToken cancellationToken = default)
    {
        workItemId = workItemId.Trim();

        var codeLinkEntity = await _dbContext.CodeLinks
            .Include(c => c.WorkItem)
            .FirstOrDefaultAsync(
                c => c.Id == codeLink.Id &&
                     c.WorkItemId == workItemId
            );

        if (codeLinkEntity == null)
            return;

        CodeLinkMapper.UpdateEntity(codeLink, codeLinkEntity);
    }

    public async Task RemoveCodeLinkFromWorkItemAsync(
        string workItemId,
        string codeLinkId,
        CancellationToken cancellationToken = default)
    {
        codeLinkId = codeLinkId.Trim();
        workItemId = workItemId.Trim();

        var codeLinkEntity = await _dbContext.CodeLinks
           .Include(c => c.WorkItem)
           .FirstOrDefaultAsync(
               c => c.Id == codeLinkId &&
                    c.WorkItemId == workItemId
           );

        if (codeLinkEntity == null)
            return;

        _dbContext.CodeLinks.Remove(codeLinkEntity);
    }

    public async Task<CodeLink?> GetCodeLinkOnWorkItemByIdAsync(
        string workItemId,
        string codeLinkId,
        CancellationToken cancellationToken = default)
    {
        codeLinkId = codeLinkId.Trim();
        workItemId = workItemId.Trim();

        var codeLinkEntity = await _dbContext.CodeLinks
            .FirstOrDefaultAsync(
                 r => r.Id == codeLinkId &&
                 r.WorkItemId == workItemId
            );
        return codeLinkEntity == null ? null : CodeLinkMapper.ToDomain(codeLinkEntity);
    }

    // attachments
    public async Task AddAttachmentOnWorkItemAsync(
        string tenantId,
        string projectId,
        string workItemId,
        Attachment attachment,
        CancellationToken cancellationToken = default)
    {
        tenantId = tenantId.Trim();
        projectId = projectId.Trim();
        workItemId = workItemId.Trim();

        var workItemEntity = await _dbContext.WorkItems
            .Include(r => r.Attachments)
            .FirstOrDefaultAsync(
            r => r.ProjectId == projectId &&
                 r.Project.TenantId == tenantId &&
                 r.Id == workItemId
            );
        if (workItemEntity == null)
            return;

        var attachmentEntity = AttachmentMapper.ToEntity(attachment, workItemId);
        workItemEntity.Attachments.Add(attachmentEntity);
    }

    public async Task RemoveAttachmentFromWorkItemAsync(
        string workItemId,
        string attachmentId,
        CancellationToken cancellationToken = default)
    {
        attachmentId = attachmentId.Trim();
        workItemId = workItemId.Trim();

        var attachmentEntity = await _dbContext.Attachments
           .Include(c => c.WorkItem)
           .FirstOrDefaultAsync(
               c => c.Id == attachmentId &&
                    c.WorkItemId == workItemId
           );

        if (attachmentEntity == null)
            return;

        _dbContext.Attachments.Remove(attachmentEntity);
    }

    public async Task<Attachment?> GetAttachmentOnWorkItemByIdAsync(
        string workItemId,
        string attachmentId,
        CancellationToken cancellationToken = default)
    {
        attachmentId = attachmentId.Trim();
        workItemId = workItemId.Trim();

        var attachmentEntity = await _dbContext.Attachments
            .FirstOrDefaultAsync(
                 r => r.Id == attachmentId &&
                 r.WorkItemId == workItemId
            );
        return attachmentEntity == null ? null : AttachmentMapper.ToDomain(attachmentEntity);
    }
}