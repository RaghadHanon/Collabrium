using Application.Common;
using Application.Features.Users.DTOs;
using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class UsersRepository : IUsersRepository
{
    private readonly UserManager<ApplicationUserEntity> _userManager;
    private readonly AppDbContext _dbContext;

    public UsersRepository(UserManager<ApplicationUserEntity> userManager, AppDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<User?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var entity = await _userManager.Users
            .Include(u => u.Tenant)
            .Include(u => u.UserContactInfo)
            .Include(u => u.ProfileImage)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return entity == null ? null : UserMapper.ToDomain(entity);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var entity = await _userManager.Users
            .Include(u => u.Tenant)
            .Include(u => u.UserContactInfo)
            .Include(u => u.ProfileImage)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        return entity == null ? null : UserMapper.ToDomain(entity);
    }

    public async Task<User?> GetByUserNameAsync(string username, CancellationToken cancellationToken = default)
    {
        var entity = await _userManager.Users
            .Include(u => u.Tenant)
            .Include(u => u.UserContactInfo)
            .Include(u => u.ProfileImage)
            .FirstOrDefaultAsync(u => u.UserName == username, cancellationToken);

        return entity == null ? null : UserMapper.ToDomain(entity);
    }

    public async Task<List<User>> GetUsersByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var entities = await _userManager.Users
            .Include(u => u.Tenant)
            .Include(u => u.UserContactInfo)
            .Include(u => u.ProfileImage)
            .Where(u => u.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        return entities.Select(UserMapper.ToDomain).ToList();
    }

    public async Task<IdentityResult> CreateUserAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        // Identity requires an Identity entity
        var entity = UserMapper.ToEntity(user);

        var result = await _userManager.CreateAsync(entity, password);
        return result;
    }

    public async Task<IdentityResult> DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var entity = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        return entity == null
            ? IdentityResult.Failed(new IdentityError { Description = "User not found." })
            : await _userManager.DeleteAsync(entity);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _userManager.Users.AnyAsync(u => u.Email == email, cancellationToken) == false;
    }

    public async Task<bool> IsUserNameUniqueAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _userManager.Users.AnyAsync(u => u.UserName == username, cancellationToken) == false;
    }

    public async Task<PagedList<User, DateTime?>> GetUsersAsync(
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

        if (parameters.Cursor.HasValue)
            query = query.Where(u => u.CreatedAt >= parameters.Cursor.Value);

        var entities = await query
            .OrderBy(u => u.CreatedAt)
            .Take(parameters.PageSize + 1)
            .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;
        if (entities.Count > parameters.PageSize)
        {
            nextCursor = entities.Last().CreatedAt;
            entities.RemoveAt(entities.Count - 1);
        }

        var domainUsers = entities.Select(UserMapper.ToDomain).ToList();

        return new PagedList<User, DateTime?>
        {
            Items = domainUsers,
            NextCursor = nextCursor
        };
    }

    public async Task<List<User>> GetByIdsAsync(IEnumerable<string> userIds, CancellationToken cancellationToken = default)
    {
        if (userIds == null) return new List<User>();

        var ids = userIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (ids.Count == 0) return new List<User>();

        var entities = await _userManager.Users
            .Include(u => u.Tenant)
            .Include(u => u.UserContactInfo)
            .Include(u => u.ProfileImage)
            .Where(u => ids.Contains(u.Id))
            .ToListAsync(cancellationToken);

        return entities.Select(UserMapper.ToDomain).ToList();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<ApplicationUserEntity> ApplyFilters(
        IQueryable<ApplicationUserEntity> query,
        UsersSearchAndFilterParametersDto parameters)
    {
        if (!string.IsNullOrWhiteSpace(parameters.TenantId))
            query = query.Where(u => u.TenantId == parameters.TenantId);

        if (parameters.Department.HasValue)
            query = query.Where(u => u.Department == parameters.Department.Value);

        if (!string.IsNullOrWhiteSpace(parameters.NameContains))
            query = query.Where(u => u.FullName.Contains(parameters.NameContains));

        if (parameters.DateOfBirth.HasValue)
            query = query.Where(u => u.DateOfBirth >= parameters.DateOfBirth.Value);

        if (parameters.LastLoginAfter.HasValue)
            query = query.Where(u => u.LastLoginAt >= parameters.LastLoginAfter.Value);

        if (parameters.CreatedAfter.HasValue)
            query = query.Where(u => u.CreatedAt >= parameters.CreatedAfter.Value);

        if (parameters.CreatedBefore.HasValue)
            query = query.Where(u => u.CreatedAt <= parameters.CreatedBefore.Value);

        if (!string.IsNullOrWhiteSpace(parameters.PhoneNumber))
            query = query.Where(u => u.PhoneNumber == parameters.PhoneNumber);

        return query;
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var entity = await _userManager.Users
            .Include(u => u.Tenant)
            .Include(u => u.UserContactInfo)
            .Include(u => u.ProfileImage)
            .FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);

        if (entity == null)
            throw new InvalidOperationException($"User '{user.Id}' was not found.");

        UserMapper.UpdateEntity(user, entity);
        await _userManager.UpdateAsync(entity);
    }
}
