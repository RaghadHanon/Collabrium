using Application.Common;
using Application.Features.Projects.DTOs.Boards;
using Application.Features.WorkItems.DTOs;
using Application.Features.WorkItems.DTOs.Attachment;
using Application.Features.WorkItems.DTOs.Comments;
using Application.Features.WorkItems.DTOs.Relations;
using Domain.Enums;
using Domain.Models;

namespace Application.Interfaces.ReadModels;

public interface IWorkItemsReadModel
{
    Task<WorkItemDto?> GetByIdAsync(string tenantId, string projectId, string workItemId, CancellationToken ct = default);
    Task<string?> GetWorkItemDisplayNameAsync(
        string tenantId,
        string projectId,
        string workItemId,
        CancellationToken cancellationToken = default);
    Task<BoardDto> GetBoardItemsAsync(
        WorkItemsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default);
    Task<PagedList<WorkItemListItemDto, DateTime?>> GetWorkItemsByTypeAsync(
        string tenantId,
        string projectId,
        WorkItemType type,
        WorkItemsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default);
    List<WorkItemStatus> GetWorkItemWorkflowByType(WorkItemType type);
    Task<PagedList<WorkItemListItemDto, DateTime?>> GetWorkItemsAsync(
        WorkItemsSearchAndFilterParametersDto parameters,
        CancellationToken ct = default);

    Task<PagedList<WorkItemListItemDto, DateTime?>> GetBacklogAsync(
        string tenantId,
        string projectId,
        WorkItemsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default);

    //relations
    Task<List<WorkItemRelationDto>> GetRelationsByTypeAsync(
         string tenantId,
         string projectId,
         string workItemId,
         WorkItemRelationType? relationType = null,
         CancellationToken cancellationToken = default);

    Task<List<WorkItemSnapshot>> GetChildrenAsync(
        string tenantId,
        string projectId,
        string workItemId,
        CancellationToken cancellationToken = default);

    Task<WorkItemRelationDto?> GetRelationByIdAsync(
       string projectId,
       string sourceWorkItemId,
       string relationId,
       CancellationToken cancellationToken = default);

    Task<List<RelationParticipantWorkItemDto>?> GetRelationCandidates(
       string tenantId,
       string projectId,
       string sourceWorkItemId,
       WorkItemRelationType relationType,
       CancellationToken cancellationToken = default);

    // comments
    Task<PagedList<CommentDto, DateTime?>> GetWorkItemComments(
        CommentsQueryParametersDto parameters,
        string workItemId,
        CancellationToken cancellationToken = default);

    Task<CommentDto?> GetWorkItemComment(
     string workItemId,
     string commentId,
     CancellationToken cancellationToken = default);

    // attachments
    Task<List<AttachmentDto>> GetWorkItemAttachments(
        string workItemId,
        CancellationToken cancellationToken = default);
}