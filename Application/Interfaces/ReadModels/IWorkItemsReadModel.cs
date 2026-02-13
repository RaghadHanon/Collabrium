using Application.Common;
using Application.Features.WorkItems.DTOs;
using Domain.Enums;
using Domain.Models;

namespace Application.Interfaces.ReadModels;

public interface IWorkItemsReadModel
{
    Task<WorkItemDto?> GetByIdAsync(string tenantId, string projectId, string workItemId, CancellationToken ct = default);
    Task<List<WorkItemSnapshot>?> GetChildrenAsync(string tenantId, string projectId, string workItemId,
        CancellationToken cancellationToken = default);
    Task<List<WorkItemListItemDto>> GetBoardItemsAsync(
        string tenantId,
        string projectId,
        string? sprintId,
        CancellationToken cancellationToken = default);
    Task<List<WorkItemListItemDto>> GetWorkItemsByTypeAsync(
            string tenantId,
            string projectId,
            WorkItemType type,
            string? sprintId,
            CancellationToken cancellationToken = default);
    List<WorkItemStatus> GetAllowedStatusesByType(WorkItemType type);
    Task<PagedList<WorkItemListItemDto, DateTime?>> GetWorkItemsAsync(
        WorkItemsSearchAndFilterParametersDto parameters,
        CancellationToken ct = default);
}