using Domain.Models;

namespace Application.Interfaces.Repositories;

public interface ITeamsRepository
{
    Task AddAsync(
        Team team,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Team team,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveAsync(
        string tenantId,
        string teamId,
        CancellationToken cancellationToken = default);

    Task<Team?> GetByIdAsync(
        string tenantId,
        string teamId,
        CancellationToken cancellationToken = default);

    Task<bool> IsTeamExists(
        string tenantId,
        string teamId);

    Task AddMembersAsync(
        string tenantId,
        string teamId,
        IReadOnlyCollection<string> memberIds,
        CancellationToken cancellationToken = default);

    Task RemoveMembersAsync(
        string tenantId,
        string teamId,
        IReadOnlyCollection<string> memberIds,
        CancellationToken cancellationToken = default);

    Task<bool> IsNameUniqueAsync(
        string tenantId,
        string name,
        CancellationToken cancellationToken = default);

    Task<bool> IsUserLeadingAnyTeamAsync(
        string tenantId,
        string userId,
        CancellationToken cancellationToken = default);
}