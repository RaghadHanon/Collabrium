using Application.Common;
using Application.Features.WorkItems.DTOs;
using Domain.Enums;
using Domain.Models;

namespace Application.Interfaces.Repositories;

public interface IWorkItemsRepository
{
    void Add(WorkItem workItem, CancellationToken cancellationToken = default);
    Task UpdateAsync(WorkItem workItem, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(string tenantId, string projectId, string workItemId, CancellationToken cancellationToken = default);
    Task<bool> IsExistAsync(string tenantId, string projectId, string workItemId, CancellationToken cancellationToken = default);
    Task MoveWorkItemsFromSprintToBacklogAsync(string tenantId, string sprintId, string projectId, CancellationToken cancellationToken = default);

    Task<List<WorkItem>> GetWorkItemsListFromIds(string tenantId, string projectId, string sprintId,
        string[] workItemIds, CancellationToken cancellationToken);
    Task<WorkItem?> GetByIdAsync(string tenantId, string projectId, string workItemId, bool includeChildren = true, CancellationToken cancellationToken = default);
    Task<PagedList<WorkItem, DateTime?>> GetPagedAsync(
        WorkItemsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default);

    // relations
    Task PersistRelationAsync(
        string tenantId,
        string projectId,
        string sourceWorkItemId,
        string targetWorkItemId,
        WorkItemRelationType relationType,
        CancellationToken ct = default);

    Task<bool> DeleteRelationAsync(
        string tenantId,
        string projectId,
        string sourceWorkItemId,
        string relationId,
        CancellationToken ct = default);

    // comments
    Task AddCommentOnWorkItemAsync(
        string tenantId,
        string projectId,
        string workItemId,
        WorkItemComment comment,
        CancellationToken cancellationToken = default);

    Task<WorkItemComment?> GetCommentOnWorkItemByIdAsync(
        string workItemId,
        string commentId,
        CancellationToken cancellationToken = default);

    Task UpdateCommentOfWorkItemAsync(
        string workItemId,
        WorkItemComment comment,
        CancellationToken cancellationToken = default);

    Task RemoveCommentFromWorkItemAsync(
        string workItemId,
        string commentId,
        CancellationToken cancellationToken = default);

    //code links
    Task RemoveCodeLinkFromWorkItemAsync(
        string workItemId,
        string codeLinkId,
        CancellationToken cancellationToken = default);
    Task<CodeLink?> GetCodeLinkOnWorkItemByIdAsync(
            string workItemId,
            string codeLinkId,
            CancellationToken cancellationToken = default);
    Task UpdateCodeLinkOfWorkItemAsync(
        string workItemId,
        CodeLink codeLink,
        CancellationToken cancellationToken = default);
    Task AddCodeLinkOnWorkItemAsync(
        string tenantId,
        string projectId,
        string workItemId,
        CodeLink codeLink,
        CancellationToken cancellationToken = default);

    // attachments
    Task AddAttachmentOnWorkItemAsync(
            string tenantId,
            string projectId,
            string workItemId,
            Attachment attachment,
            CancellationToken cancellationToken = default);
    Task RemoveAttachmentFromWorkItemAsync(
        string workItemId,
        string attachmentId,
        CancellationToken cancellationToken = default);
    Task<Attachment?> GetAttachmentOnWorkItemByIdAsync(
        string workItemId,
        string attachmentId,
        CancellationToken cancellationToken = default);

}