using Application.Common;
using Application.Features.Identity.DTOs;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Interfaces;
public interface IUsersRepository
{
    Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password);
    Task<IdentityResult> DeleteUserAsync(ApplicationUser user);
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<ApplicationUser?> GetByIdAsync(string userId);
    Task<ApplicationUser?> GetByUserNameAsync(string username);
    Task<List<Image>> GetUserProfileImagesAsync(string userId);
    Task<List<ApplicationUser>> GetUsersByTenantAsync(string tenantId);
    Task<bool> IsEmailUniqueAsync(string email);
    Task<bool> IsUserNameUniqueAsync(string username);
    Task<IdentityResult> UpdateUserAsync(ApplicationUser user);

    Task<PagedList<ApplicationUser, DateTime?>> GetUsersAsync(
         UsersSearchAndFilterParametersDto parameters,
         CancellationToken cancellationToken = default);
}