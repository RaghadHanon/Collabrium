using Application.Common;
using Application.Features.Teams.DTOs;
using Application.Features.Users.DTOs;

namespace Application.Interfaces.ReadModels;

public interface ITeamsReadModel
{
    Task<TeamDto?> GetByIdAsync(string tenantId, string teamId, CancellationToken cancellationToken = default);

    Task<PagedList<TeamDto, DateTime?>> GetTeamsAsync(
        TeamsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default);
    Task<string?> GetTeamDisplayNameAsync(
        string tenantId,
        string teamId,
        CancellationToken cancellationToken = default);
    Task<PagedList<TeamDto, DateTime?>> GetUserTeamsAsync(
        string userId,
        TeamsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default);

    Task<List<UserSnapshotDto>> GetTeamMembersSnapshotAsync(string tenantId, string teamId, CancellationToken cancellationToken = default);

    Task<List<UserDto>> GetCandidatesMembers(string tenantId, string teamId, CancellationToken cancellationToken = default);

    Task<TeamFullInfoDto?> GetTeamFullAsync(string tenantId, string teamId, CancellationToken cancellationToken = default);
    Task<List<string>> GetUserTeamIdsAsync(
      string tenantId,
      string userId,
      CancellationToken cancellationToken = default);
    Task<List<string>> GetTeamMemberIdsAsync(
    string tenantId,
    string teamId,
    CancellationToken cancellationToken = default);

}