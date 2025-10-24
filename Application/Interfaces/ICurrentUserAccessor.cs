using Domain.Entities;

namespace Application.Interfaces;

public interface ICurrentUserAccessor
{
    Task<ApplicationUser?> GetUserAsync();

    string? GetUserId();
}