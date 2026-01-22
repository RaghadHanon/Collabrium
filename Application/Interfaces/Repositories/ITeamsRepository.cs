using Application.Common;
using Application.Features.Teams.DTOs;
using Domain.Entities;

namespace Application.Interfaces.Repositories;
public interface ITeamsRepository
{
    Task AddTeamAsync(Team team, CancellationToken cancellationToken = default);
    Task<Team?> GetByIdAsync(string tenantId, string teamId, CancellationToken cancellationToken = default);
    Task<Team?> GetTeamByIdAsync(string tenantId, string id, CancellationToken cancellationToken = default);
    Task<PagedList<Team, DateTime?>> GetTeamsAsync(TeamsSearchAndFilterParametersDto parameters, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Team>> GetTeamsByTenantIdAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<bool> IsTeamNameUniqueAsync(string tenantId, string name, CancellationToken cancellationToken = default);
    Task<bool> RemoveTeamAsync(string id, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}