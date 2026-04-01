using Application.Common;
using Application.Features.Projects.DTOs;
using Domain.Models;

namespace Application.Interfaces.ReadModels;

public interface IProjectsReadModel
{
    Task<ProjectDto?> GetByIdAsync(string tenantId, string projectId, CancellationToken cancellationToken = default);

    Task<List<WorkItemSnapshot>> GetProjectWorkItemSnapshotAsync(string tenantId, string projectId,
        CancellationToken cancellationToken = default);
    Task<PagedList<ProjectDto, DateTime?>> GetUserProjectsAsync(
        string userId,
        ProjectsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default);
    Task<PagedList<ProjectDto, DateTime?>> GetProjectsAsync(
        ProjectsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default);
}
