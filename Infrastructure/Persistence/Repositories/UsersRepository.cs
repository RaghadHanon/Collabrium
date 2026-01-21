using Application.Common;
using Application.Features.Users.DTOs;
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
            .Include(u => u.UserContactInfo)
            .Include(u => u.ProfileImage)
            .FirstOrDefaultAsync(u => u.Id == userId);
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
            .Include(u => u.Tenant)
            .Include(u => u.UserContactInfo)
            .Include(u => u.ProfileImage)
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

    public async Task<PagedList<ApplicationUser, DateTime?>> GetUsersAsync(
         UsersSearchAndFilterParametersDto parameters,
         CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users
            .Include(u => u.Tenant)
            .Include(u => u.UserContactInfo)
            .Include(u => u.ProfileImage)
            .AsNoTracking()
            .AsQueryable();

        query = ApplyFilters(query, parameters);
        return await ApplyPagination(parameters, query, cancellationToken);
    }

    private static async Task<PagedList<ApplicationUser, DateTime?>> ApplyPagination(
        UsersSearchAndFilterParametersDto parameters, IQueryable<ApplicationUser> query,
        CancellationToken cancellationToken)
    {
        if (parameters.Cursor.HasValue)
            query = query.Where(c => c.CreatedAt >= parameters.Cursor.Value);

        var items = await query
            .Take(parameters.PageSize + 1)
            .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;
        if (items.Count > parameters.PageSize)
        {
            nextCursor = items.Last().CreatedAt;
            items.RemoveAt(items.Count - 1);
        }

        return new PagedList<ApplicationUser, DateTime?>
        {
            Items = items,
            NextCursor = nextCursor
        };
    }

    private static IQueryable<ApplicationUser> ApplyFilters(
        IQueryable<ApplicationUser> query,
        UsersSearchAndFilterParametersDto parameters)
    {
        if (!string.IsNullOrWhiteSpace(parameters.TenantId))
            query = query.Where(c => c.TenantId == parameters.TenantId);

        if (parameters.Department.HasValue)
            query = query.Where(c => c.Department == parameters.Department.Value);

        if (!string.IsNullOrWhiteSpace(parameters.NameContains))
            query = query.Where(c => c.FullName.Contains(parameters.NameContains));

        if (parameters.DateOfBirth.HasValue)
            query = query.Where(c => c.DateOfBirth >= parameters.DateOfBirth.Value);

        if (parameters.LastLoginAfter.HasValue)
            query = query.Where(c => c.LastLoginAt >= parameters.LastLoginAfter.Value);

        if (parameters.CreatedAfter.HasValue)
            query = query.Where(c => c.CreatedAt >= parameters.CreatedAfter.Value);

        if (parameters.CreatedBefore.HasValue)
            query = query.Where(c => c.CreatedAt <= parameters.CreatedBefore.Value);

        if (!string.IsNullOrWhiteSpace(parameters.PhoneNumber))
            query = query.Where(c => c.PhoneNumber == parameters.PhoneNumber);

        return query;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
