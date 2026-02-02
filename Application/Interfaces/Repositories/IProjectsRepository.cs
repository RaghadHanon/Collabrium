using Application.Common;
using Application.Common.DTOs;
using Domain.Models;

namespace Application.Interfaces.Repositories;

public interface IProjectsRepository
{
    Task AddAsync(Project project, CancellationToken cancellationToken = default);
    Task UpdateAsync(Project project, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(string tenantId, string projectId, CancellationToken cancellationToken = default);

    Task<Project?> GetByIdAsync(string tenantId, string projectId, CancellationToken cancellationToken = default);

    Task<bool> IsNameUniqueAsync(string tenantId, string name, CancellationToken cancellationToken = default);

    Task<PagedList<Project, DateTime?>> GetPagedAsync(
        string tenantId,
        QueryParametersDto<DateTime?> parameters,
        CancellationToken cancellationToken = default);
}
