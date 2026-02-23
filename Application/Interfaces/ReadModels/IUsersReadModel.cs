using Application.Common;
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
}
