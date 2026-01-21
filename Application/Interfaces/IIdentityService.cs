using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces;
public interface IIdentityService
{
    Task<(string Token, string Id, string TenantId)> AuthenticateAsync(string email, string password);
    Task ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task<IList<string>> GetRolesAsync(ApplicationUser user);
    Task<ApplicationUser?> GetUserAsync(string email);
    Task<bool> IsEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    Task<ApplicationUser> RegisterUserAsync(string fullName, Department department, string password, DateOnly dateOfBirth, UserRole role, string tenantId, string? email = null, string? phoneNumber = null);
    Task ReSendConfirmationEmail(string email, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(string userId, string token, string newPassword);
    Task SendConfirmationEmail(ApplicationUser user, CancellationToken cancellationToken = default);
    Task SendPasswordResetEmailAsync(string email, CancellationToken cancellationToken = default);
    Task UpdateUserAsync(ApplicationUser user);
}