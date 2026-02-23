using Application.Common;
using Application.Features.Sprints.DTOs;

namespace Application.Interfaces.ReadModels;

public interface ISprintsReadModel
{
    Task<SprintDto?> GetByIdAsync(string tenantId, string sprintId, string? projectId = null, CancellationToken cancellationToken = default);

    Task<PagedList<SprintDto, DateTime?>> GetSprintsAsync(
        SprintsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default);
}