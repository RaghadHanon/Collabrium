using Application.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Mappers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Infrastructure.Security;

public class CurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AppDbContext _dbContext;

    public CurrentUserAccessor(IHttpContextAccessor httpContextAccessor, AppDbContext dbContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
    }

    public string? GetUserId()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public async Task<User?> GetUserAsync(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        var entity = await _dbContext.Users
            .Include(u => u.Tenant)
            .Include(u => u.UserContactInfo)
            .Include(u => u.ProfileImage)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return entity == null ? null : UserMapper.ToDomain(entity);
    }
}
