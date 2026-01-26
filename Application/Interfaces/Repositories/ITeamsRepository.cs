using Application.Common;
using Application.Features.Teams.DTOs;
using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface ITeamsRepository
{
    // Write
    Task AddAsync(Team team, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(string tenantId, string teamId, CancellationToken cancellationToken = default);
    Task UpdateAsync(Team team, CancellationToken cancellationToken = default);

    Task ToggleActivationAsync(string tenantId, string teamId, CancellationToken cancellationToken = default);

    Task AddMembersAsync(string tenantId, string teamId, IReadOnlyCollection<string> memberIds, CancellationToken cancellationToken = default);
    Task RemoveMembersAsync(string tenantId, string teamId, IReadOnlyCollection<string> memberIds, CancellationToken cancellationToken = default);

    // Read (Domain)
    Task<Team?> GetByIdAsync(string tenantId, string teamId, bool includeMembers = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Team>> GetByTenantAsync(string tenantId, bool includeMembers = false, CancellationToken cancellationToken = default);

    Task<PagedList<Team, DateTime?>> GetPagedAsync(
        TeamsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default);

    // Rules checks
    Task<bool> IsNameUniqueAsync(string tenantId, string name, CancellationToken cancellationToken = default);
}