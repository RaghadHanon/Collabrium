using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class UsersRepository : IUsersRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _dbContext;

    public UsersRepository(UserManager<ApplicationUser> userManager,
        AppDbContext context)
    {
        _userManager = userManager;
        _dbContext = context;
    }

    public async Task<ApplicationUser?> GetByIdAsync(string userId)
    {
        return await _userManager.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<List<Image>> GetUserProfileImagesAsync(string userId)
    {
        return await _dbContext.Images
            .Where(img => img.ApplicationUserId == userId)
            .ToListAsync();
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<ApplicationUser?> GetByUserNameAsync(string username)
    {
        return await _userManager.FindByNameAsync(username);
    }

    public async Task<List<ApplicationUser>> GetUsersByTenantAsync(string tenantId)
    {
        return await _userManager.Users
            .Where(u => u.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password)
    {
        return await _userManager.CreateAsync(user, password);
    }

    public async Task<IdentityResult> UpdateUserAsync(ApplicationUser user)
    {
        return await _userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> DeleteUserAsync(ApplicationUser user)
    {
        return await _userManager.DeleteAsync(user);
    }

    public async Task<bool> IsEmailUniqueAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email) == null;
    }

    public async Task<bool> IsUserNameUniqueAsync(string username)
    {
        return await _userManager.FindByNameAsync(username) == null;
    }
}
