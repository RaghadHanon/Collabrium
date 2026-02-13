using Domain.Enums;
using Domain.Models;

namespace Application.Interfaces.Services;

public interface ICurrentUserAccessor
{
    string? GetUserId();
    Task<User?> GetCurrentUserAsync(CancellationToken cancellationToken = default);
    Task<UserRole?> GetUserRole(string userId, CancellationToken cancellationToken);
}
