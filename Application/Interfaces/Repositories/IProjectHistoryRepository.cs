using Application.Common;
using Application.Features.ProjectHistories.DTOs;
using Domain.Models;

namespace Application.Interfaces.Repositories;

public interface IProjectHistoryRepository
{
    void Add(ProjectHistory history, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProjectHistory>> GetByProjectIdAsync(
        string tenantId,
        string projectId,
        CancellationToken cancellationToken = default);

    Task<PagedList<ProjectHistory, DateTime?>> GetPagedAsync(
        ProjectHistorySearchAndFiltersParametersDto parameters,
        string projectId,
        string tenantId,
        CancellationToken cancellationToken = default);
}