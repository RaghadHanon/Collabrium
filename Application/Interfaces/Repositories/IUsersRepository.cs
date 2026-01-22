using Application.Common;
using Application.Features.Users.DTOs;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Interfaces.Repositories;
public interface IUsersRepository
{
    Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password);
    Task<IdentityResult> DeleteUserAsync(ApplicationUser user);
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<ApplicationUser?> GetByIdAsync(string userId);
    Task<ApplicationUser?> GetByUserNameAsync(string username);
    Task<List<ApplicationUser>> GetUsersByTenantAsync(string tenantId);
    Task<bool> IsEmailUniqueAsync(string email);
    Task<bool> IsUserNameUniqueAsync(string username);
    Task<IdentityResult> UpdateUserAsync(ApplicationUser user);
    Task<PagedList<ApplicationUser, DateTime?>> GetUsersAsync(
         UsersSearchAndFilterParametersDto parameters,
         CancellationToken cancellationToken = default);
    Task<List<ApplicationUser>> GetByIdsAsync(
        IEnumerable<string> userIds,
        CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}