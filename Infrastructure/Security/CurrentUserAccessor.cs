using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Infrastructure.Security;

public class CurrentUserAccessor(IHttpContextAccessor httpContextAccessor
    , AppDbContext dbContext) : ICurrentUserAccessor
{
    public async Task<ApplicationUser?> GetUserAsync()
    {
        var userId = GetUserId();
        return string.IsNullOrEmpty(userId)
            ? null
            : await dbContext.Users
             .Include(u => u.Tenant)
             .FirstOrDefaultAsync(x => x.Id == userId);
    }

    public string? GetUserId()
    {
        return httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}