using Application.Common;
using Application.Features.Users.DTOs;
using Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace Application.Interfaces.Repositories;

public interface IUsersRepository
{
    Task<User?> GetByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUserNameAsync(string username, CancellationToken cancellationToken = default);
    Task<List<User>> GetUsersByTenantAsync(string tenantId, CancellationToken cancellationToken = default);

    Task<UserWithTenantDto?> GetUserWithTenantDtoAsync(string tenantId, CancellationToken cancellationToken = default);

    Task<IdentityResult> CreateUserAsync(User user, string password, CancellationToken cancellationToken = default);
    Task<IdentityResult> DeleteUserAsync(string userId, CancellationToken cancellationToken = default);

    Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> IsUserNameUniqueAsync(string username, CancellationToken cancellationToken = default);

    Task<PagedList<User, DateTime?>> GetUsersAsync(
        UsersSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default);

    Task<List<User>> GetByIdsAsync(IEnumerable<string> userIds, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
