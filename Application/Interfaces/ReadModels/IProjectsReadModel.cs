using Application.Common;
using Application.Features.Projects.DTOs;
using Application.Features.WorkItems.DTOs;
using Domain.Models;

namespace Application.Interfaces.ReadModels;

public interface IProjectsReadModel
{
    Task<ProjectDto?> GetByIdAsync(string tenantId, string projectId, CancellationToken cancellationToken = default);

    Task<List<WorkItemSnapshot>> GetProjectWorkItemSnapshotAsync(string tenantId, string projectId,
        CancellationToken cancellationToken = default);

    Task<PagedList<ProjectDto, DateTime?>> GetProjectsAsync(
        ProjectsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkItemListItemDto>> GetBacklogAsync(
        string tenantId,
        string projectId,
        CancellationToken cancellationToken = default);
}
