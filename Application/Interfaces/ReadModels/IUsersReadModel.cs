using Application.Common;
using Application.Features.Tags.DTOs;
using Application.Features.Users.DTOs;

namespace Application.Interfaces.ReadModels;

public interface IUsersReadModel
{
    Task<UserWithTenantDto?> GetUserWithTenantAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> UserExistsAsync(
        string userId,
        CancellationToken cancellationToken = default);
    Task<UserDto?> GetUserByIdAsync(
        string userId,
        CancellationToken cancellationToken = default);
    Task<PagedList<UserDto, DateTime?>> GetUsersAsync(
        UsersSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default);
    Task<PagedList<UserDto, DateTime?>> GetAssignableUsersAsync(
     AssignableUsersSearchAndFilterParametersDto parameters,
     CancellationToken ct);
    Task<UserSnapshotDto?> GetUserSnapshotByIdAsync(
      string userId,
      CancellationToken ct);
    Task<List<TagDto>> GetUserSkillsAsync(
    string userId,
    CancellationToken cancellationToken = default);
    Task<List<string>> GetTenantUserIdsAsync(
    string tenantId,
    CancellationToken cancellationToken = default);

    Task<List<string>> GetUserIdsByRoleAsync(
        string tenantId,
        string roleName,
        CancellationToken cancellationToken = default);
    Task<List<UserDto>> GetManagersAsync(
    string tenantId,
    CancellationToken ct = default);
}
