using Application.Common;
using Application.Features.ProjectHistories.DTOs;

namespace Application.Interfaces.ReadModels;
public interface IProjectHistoriesReadModel
{
    Task<PagedList<ProjectHistoryDto, DateTime?>> GetPagedListAsync(
        ProjectHistorySearchAndFiltersParametersDto parameters,
        string projectId,
        string tenantId,
        CancellationToken ct = default);
}