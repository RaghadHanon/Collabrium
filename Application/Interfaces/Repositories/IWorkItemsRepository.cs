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
    Task<bool> UpdateRelationAsync(
        string tenantId,
        string projectId,
        string relationId,
        string newTargetWorkItemId,
        WorkItemRelationType newRelationType,
        CancellationToken ct = default);
    Task<bool> DeleteRelationAsync(
        string tenantId,
        string projectId,
        string relationId,
        CancellationToken ct = default);
}