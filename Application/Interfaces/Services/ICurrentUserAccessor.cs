using Domain.Models;

namespace Application.Interfaces.Services;

public interface ICurrentUserAccessor
{
    string? GetUserId();
    Task<User?> GetUserAsync(CancellationToken cancellationToken = default);
}
