using Application.Interfaces.Services;
using Domain.Enums;
using Domain.Models;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.Mappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Infrastructure.Security;

public sealed class CurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AppDbContext _dbContext;
    private readonly UserManager<ApplicationUserEntity> _userManager;

    public CurrentUserAccessor(IHttpContextAccessor httpContextAccessor, AppDbContext dbContext,
        UserManager<ApplicationUserEntity> userManager)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    public string? GetUserId()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public async Task<User?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        var entity = await _dbContext.Users
            .AsNoTracking()
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return entity is null ? null : UserMapper.ToDomain(entity);
    }

    public async Task<UserRole?> GetUserRole(string userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);
        var role = Enum.TryParse(roles.FirstOrDefault(), true, out UserRole userRole) ? userRole : (UserRole?)null; 

        return role;
    }
}