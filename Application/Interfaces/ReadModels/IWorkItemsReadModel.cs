using Application.Common;
using Application.Features.WorkItems.DTOs;

namespace Application.Interfaces.ReadModels;

public interface IWorkItemsReadModel
{
    Task<WorkItemDto?> GetByIdAsync(string tenantId, string projectId, string workItemId, CancellationToken ct = default);

    Task<PagedList<WorkItemListItemDto, DateTime?>> GetWorkItemsAsync(
        WorkItemsSearchAndFilterParametersDto parameters,
        CancellationToken ct = default);
}