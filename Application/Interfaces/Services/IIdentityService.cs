using Domain.Enums;
using Domain.Models;

namespace Application.Interfaces.Services;

public interface IIdentityService
{
    Task<User> RegisterUserAsync(
        string fullName,
        Department department,
        string password,
        DateOnly dateOfBirth,
        UserRole role,
        string tenantId,
        string email,
        string phoneNumber,
        CancellationToken cancellationToken = default);

    Task<(string Token, string Id, string TenantId)> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task ConfirmEmailAsync(string userId, string encodedToken, CancellationToken cancellationToken = default);

    Task ResendConfirmationEmailAsync(string email, CancellationToken cancellationToken = default);

    Task SendPasswordResetEmailAsync(string email, CancellationToken cancellationToken = default);

    Task ResetPasswordAsync(string userId, string encodedToken, string newPassword, CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(string userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
}