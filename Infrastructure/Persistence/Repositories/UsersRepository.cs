using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.Mappers;
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
        if (!TryNormalize(userId, out var id))
            return null;

        var entity = await ReadOnlyQuery()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        return entity is null ? null : UserMapper.ToDomain(entity);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (!TryNormalize(email, out var normalized))
            return null;

        var entity = await ReadOnlyQuery()
            .FirstOrDefaultAsync(u => u.Email == normalized, cancellationToken);

        return entity is null ? null : UserMapper.ToDomain(entity);
    }

    public async Task<User?> GetByUserNameAsync(string username, CancellationToken cancellationToken = default)
    {
        if (!TryNormalize(username, out var normalized))
            return null;

        var entity = await ReadOnlyQuery()
            .FirstOrDefaultAsync(u => u.UserName == normalized, cancellationToken);

        return entity is null ? null : UserMapper.ToDomain(entity);
    }

    public async Task<List<User>> GetByIdsAsync(IEnumerable<string> userIds, CancellationToken cancellationToken = default)
    {
        if (userIds is null) return new();

        var ids = userIds
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (ids.Count == 0) return new();

        var entities = await ReadOnlyQuery()
            .Where(u => ids.Contains(u.Id))
            .ToListAsync(cancellationToken);

        return entities.Select(UserMapper.ToDomain).ToList();
    }

    public async Task<bool> CreateUserAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        if (!TryNormalize(password, out _))
            throw new ArgumentNullException(nameof(password));

        var entity = UserMapper.ToEntity(user);

        var result = await _userManager.CreateAsync(entity, password);
        return !result.Succeeded ? throw new InvalidOperationException($"Failed to create user: {JoinErrors(result)}") : true;
    }

    public async Task<bool> DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (!TryNormalize(userId, out var id))
            return false;

        var entity = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (entity is null) return false;

        var result = await _userManager.DeleteAsync(entity);
        return !result.Succeeded ? throw new InvalidOperationException($"Failed to delete user: {JoinErrors(result)}") : true;
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        var entity = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);

        if (entity is null)
            throw new InvalidOperationException($"User '{user.Id}' was not found.");

        UserMapper.UpdateEntity(user, entity);

        var result = await _userManager.UpdateAsync(entity);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to update user: {JoinErrors(result)}");
    }

    public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
    {
        return !TryNormalize(email, out var normalized)
            ? false
            : !await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == normalized, cancellationToken);
    }

    public async Task<bool> IsUserNameUniqueAsync(string username, CancellationToken cancellationToken = default)
    {
        return !TryNormalize(username, out var normalized)
            ? false
            : !await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.UserName == normalized, cancellationToken);
    }

    private IQueryable<ApplicationUserEntity> ReadOnlyQuery()
    {
        return _dbContext.Users.AsNoTracking();
    }

    private static bool TryNormalize(string? value, out string normalized)
    {
        normalized = value?.Trim() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(normalized);
    }

    private static string JoinErrors(IdentityResult result)
    {
        return string.Join("; ", result.Errors.Select(e => e.Description));
    }
}