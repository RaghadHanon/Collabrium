using Application.Common;
using Application.Features.Sprints.DTOs;
using Domain.Models;

namespace Application.Interfaces.Repositories;

public interface ISprintsRepository
{
    void Add(Sprint sprint, CancellationToken cancellationToken = default);
    Task UpdateAsync(Sprint sprint, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(string tenantId, string sprintId, CancellationToken cancellationToken = default);
    Task<bool> IsExistsAsync(string tenantId, string sprintId, string? projectId = null, CancellationToken cancellationToken = default);
    Task<Sprint?> GetByIdAsync(string tenantId, string? sprintId, string? projectId = null, CancellationToken cancellationToken = default);
    Task<bool> CanSprintBeCompletedAsync(string tenantId, string sprintId, CancellationToken cancellationToken = default);
    Task<bool> ProjectHasActiveSprintAsync(string tenantId, string projectId, CancellationToken cancellationToken = default);
    Task<bool> SprintHasWorkItemsAsync(string tenantId, string sprintId, CancellationToken cancellationToken = default);
    Task<PagedList<Sprint, DateTime?>> GetPagedAsync(
        SprintsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default);
}