using Application.Common;
using Application.Features.Users.DTOs;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.Mapping;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class UsersRepository : IUsersRepository
{
    private readonly UserManager<ApplicationUserEntity> _userManager;
    private readonly AppDbContext _dbContext;

    public UsersRepository(UserManager<ApplicationUserEntity> userManager, AppDbContext dbContext)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<User?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId)) return null;

        var id = userId.Trim();

        var entity = await DomainReadOnlyQuery()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        return entity is null ? null : UserMapper.ToDomain(entity);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;

        var normalized = email.Trim();

        var entity = await DomainReadOnlyQuery()
            .FirstOrDefaultAsync(u => u.Email == normalized, cancellationToken);

        return entity is null ? null : UserMapper.ToDomain(entity);
    }

    public async Task<User?> GetByUserNameAsync(string username, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username)) return null;

        var normalized = username.Trim();

        var entity = await DomainReadOnlyQuery()
            .FirstOrDefaultAsync(u => u.UserName == normalized, cancellationToken);

        return entity is null ? null : UserMapper.ToDomain(entity);
    }

    public async Task<List<User>> GetByIdsAsync(IEnumerable<string> userIds, CancellationToken cancellationToken = default)
    {
        if (userIds is null) return new List<User>();

        var ids = userIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (ids.Count == 0) return new List<User>();

        var entities = await DomainReadOnlyQuery()
            .Where(u => ids.Contains(u.Id))
            .ToListAsync(cancellationToken);

        return entities.Select(UserMapper.ToDomain).ToList();
    }

    public async Task<bool> CreateUserAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        if (user is null) throw new ArgumentNullException(nameof(user));
        if (string.IsNullOrWhiteSpace(password)) throw new ArgumentNullException(nameof(password));

        var entity = UserMapper.ToEntity(user);
        var result = await _userManager.CreateAsync(entity, password);

        if (!result.Succeeded)
        {
            var msg = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user: {msg}");
        }

        return true;
    }

    public async Task<bool> DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        var id = userId.Trim();

        var entity = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (entity is null)
            return false;

        var result = await _userManager.DeleteAsync(entity);

        if (!result.Succeeded)
        {
            var msg = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to delete user: {msg}");
        }

        return true;
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user is null) throw new ArgumentNullException(nameof(user));

        var entity = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);

        if (entity is null)
            throw new InvalidOperationException($"User '{user.Id}' was not found.");

        UserMapper.UpdateEntity(user, entity);

        var result = await _userManager.UpdateAsync(entity);
        if (!result.Succeeded)
        {
            var msg = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to update user: {msg}");
        }
    }

    public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        var normalized = email.Trim();

        return await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == normalized, cancellationToken) == false;
    }

    public async Task<bool> IsUserNameUniqueAsync(string username, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username)) return false;
        var normalized = username.Trim();

        return await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.UserName == normalized, cancellationToken) == false;
    }

    private IQueryable<ApplicationUserEntity> DomainReadOnlyQuery()
    {
        return _dbContext.Users
            .AsNoTracking()
            .AsQueryable();
    }

    private IQueryable<ApplicationUserEntity> DomainQuery()
    {
        return _dbContext.Users
            .AsQueryable();
    }

    public Task<List<User>> GetUsersByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PagedList<User, DateTime?>> GetUsersAsync(UsersSearchAndFilterParametersDto parameters, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
