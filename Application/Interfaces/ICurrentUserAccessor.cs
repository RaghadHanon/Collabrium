using Domain.Models;

namespace Application.Interfaces;

public interface ICurrentUserAccessor
{
    string? GetUserId();
    Task<User?> GetUserAsync(CancellationToken cancellationToken = default);
}
