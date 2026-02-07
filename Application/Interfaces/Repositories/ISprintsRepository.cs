using Application.Common;
using Application.Features.Sprints.DTOs;
using Domain.Models;

namespace Application.Interfaces.Repositories;

public interface ISprintsRepository
{
    Task AddAsync(Sprint sprint, CancellationToken cancellationToken = default);
    Task UpdateAsync(Sprint sprint, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(string tenantId, string sprintId, CancellationToken cancellationToken = default);
    Task<Sprint?> GetByIdAsync(string tenantId, string sprintId, CancellationToken cancellationToken = default);
    Task<bool> HasActiveSprintAsync(string tenantId, string projectId, CancellationToken cancellationToken = default);
    Task<bool> HasWorkItemsAsync(string tenantId, string sprintId, CancellationToken cancellationToken = default);
    Task<PagedList<Sprint, DateTime?>> GetPagedAsync(
        SprintsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default);
}