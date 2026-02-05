using Application.Common;
using Application.Common.DTOs;
using Application.Features.Sprints.DTOs;

namespace Application.Interfaces.ReadModels;

public interface ISprintsReadModel
{
    Task<SprintDto?> GetByIdAsync(string tenantId, string sprintId, CancellationToken cancellationToken = default);

    Task<PagedList<SprintDto, DateTime?>> GetSprintsAsync(
        SprintsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default);
}