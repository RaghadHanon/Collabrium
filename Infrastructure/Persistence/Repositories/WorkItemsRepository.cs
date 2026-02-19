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

    public async Task<PagedList<WorkItem, DateTime?>> GetPagedAsync(
        WorkItemsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default)
    {
        var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

        var query = _dbContext.WorkItems.AsNoTracking();

        query = ApplyFilters(query, parameters);

        if (parameters.Cursor.HasValue)
            query = query.Where(x => x.CreatedAt <= parameters.Cursor.Value);

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

        sourceWorkItemDomain.AddRelation(targetWorkItemId, relationType);
        sourceWorkItemEntity.Relations.Add(CreateRelationRow(sourceWorkItemId, targetWorkItemId, relationType));

        EnsureInverse(targetWorkItemEntity, sourceWorkItemId, targetWorkItemId, relationType);
        WorkItemMapper.UpdateEntity(sourceWorkItemDomain, sourceWorkItemEntity);
    }

    private void EnsureInverse(
        WorkItemEntity targetWorkItemEntity,
        string sourceWorkItemId,
        string targetWorkItemId,
        WorkItemRelationType relationType)
    {
        var targetWorkItemDomain = WorkItemMapper.ToDomain(targetWorkItemEntity, true);
        if (relationType == WorkItemRelationType.Parent)
        {
            targetWorkItemDomain.AddRelation(sourceWorkItemId, WorkItemRelationType.Child);
            targetWorkItemEntity.Relations
                .Add(CreateRelationRow(targetWorkItemId, sourceWorkItemId, WorkItemRelationType.Child));
        }
        else if (relationType == WorkItemRelationType.Child)
        {
            targetWorkItemDomain.AddRelation(sourceWorkItemId, WorkItemRelationType.Parent);
            targetWorkItemEntity.Relations
                .Add(CreateRelationRow(targetWorkItemId, sourceWorkItemId, WorkItemRelationType.Parent));
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
                r.SourceWorkItem.Project.TenantId == tenantId, ct);

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

    // comments
    public async Task AddCommentOnWorkItemAsync(
        string tenantId,
        string projectId,
        string workItemId,
        Comment comment,
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

        var commentEntity = CommentMapper.ToEntity(comment, workItemId);
        workItemEntity.Comments.Add(commentEntity);
    }

    public async Task UpdateCommentOfWorkItemAsync(
        string workItemId,
        Comment comment, 
        CancellationToken cancellationToken = default)
    {
        workItemId = workItemId.Trim();

        var commentEntity = await _dbContext.Comments
            .Include(c => c.WorkItem)
            .FirstOrDefaultAsync(
                c => c.Id == comment.Id &&
                     c.WorkItemId == workItemId
            );

        if (commentEntity == null)
            return;

        CommentMapper.UpdateEntity(comment, commentEntity);
    }

    public async Task RemoveCommentFromWorkItemAsync(
        string workItemId,
        string commentId,
        CancellationToken cancellationToken = default)
    {
        commentId = commentId.Trim();
        workItemId = workItemId.Trim();

        var commentEntity = await _dbContext.Comments
           .Include(c => c.WorkItem)
           .FirstOrDefaultAsync(
               c => c.Id == commentId &&
                    c.WorkItemId == workItemId
           );

        if (commentEntity == null)
            return;

        _dbContext.Comments.Remove(commentEntity);
    }

    public async Task<Comment?> GetCommentOnWorkItemByIdAsync(
        string workItemId,
        string commentId,
        CancellationToken cancellationToken = default)
    {
        commentId = commentId.Trim();
        workItemId = workItemId.Trim();

        var commentEntity = await _dbContext.Comments
            .FirstOrDefaultAsync(
            r => r.Id == commentId &&
                 r.WorkItemId == workItemId
            );
        if (commentEntity == null)
            return null;

        return CommentMapper.ToDomain(commentEntity);
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
        if (codeLinkEntity == null)
            return null;

        return CodeLinkMapper.ToDomain(codeLinkEntity);
    }
}