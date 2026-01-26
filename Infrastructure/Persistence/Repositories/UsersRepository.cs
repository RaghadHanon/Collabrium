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

    // ---------------------------
    // Read
    // ---------------------------

    public async Task<User?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId)) return null;

        var entity = await UsersReadQuery()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return entity is null ? null : UserMapper.ToDomain(entity);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;

        var normalized = email.Trim();

        var entity = await UsersReadQuery()
            .FirstOrDefaultAsync(u => u.Email == normalized, cancellationToken);

        return entity is null ? null : UserMapper.ToDomain(entity);
    }

    public async Task<User?> GetByUserNameAsync(string username, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username)) return null;

        var normalized = username.Trim();

        var entity = await UsersReadQuery()
            .FirstOrDefaultAsync(u => u.UserName == normalized, cancellationToken);

        return entity is null ? null : UserMapper.ToDomain(entity);
    }

    public async Task<List<User>> GetUsersByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) return new List<User>();

        var entities = await UsersReadQuery()
            .Where(u => u.TenantId == tenantId.Trim())
            .ToListAsync(cancellationToken);

        return entities.Select(UserMapper.ToDomain).ToList();
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

        var entities = await UsersReadQuery()
            .Where(u => ids.Contains(u.Id))
            .ToListAsync(cancellationToken);

        return entities.Select(UserMapper.ToDomain).ToList();
    }

    public async Task<PagedList<User, DateTime?>> GetUsersAsync(
        UsersSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default)
    {
        parameters ??= new UsersSearchAndFilterParametersDto();

        var query = UsersReadQuery();
        query = ApplyFilters(query, parameters);

        // cursor pagination
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

        return new PagedList<User, DateTime?>
        {
            Items = entities.Select(UserMapper.ToDomain).ToList(),
            NextCursor = nextCursor
        };
    }

    // ---------------------------
    // Write (Identity)
    // ---------------------------

    public async Task<IdentityResult> CreateUserAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        if (user is null) throw new ArgumentNullException(nameof(user));
        if (string.IsNullOrWhiteSpace(password)) throw new ArgumentNullException(nameof(password));

        // إنشاء entity من domain (بالحقول اللي بتخصك)
        var entity = UserMapper.ToEntity(user);

        // IdentityResult
        return await _userManager.CreateAsync(entity, password);
    }

    public async Task<IdentityResult> DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return IdentityResult.Failed(new IdentityError { Description = "UserId is required." });

        var entity = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == userId.Trim(), cancellationToken);

        return entity is null
            ? IdentityResult.Failed(new IdentityError { Description = "User not found." })
            : await _userManager.DeleteAsync(entity);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user is null) throw new ArgumentNullException(nameof(user));

        // للتعديل لازم Tracking + Identity entity
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

    // ---------------------------
    // Validation helpers
    // ---------------------------

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

    // ---------------------------
    // Private query building
    // ---------------------------

    private IQueryable<ApplicationUserEntity> UsersReadQuery()
    {
        return _dbContext.Users
                .AsNoTracking()
                .Include(u => u.Tenant); // إذا ما بدك tenant دائماً، احكيلي وبعملها optional
    }

    private static IQueryable<ApplicationUserEntity> ApplyFilters(
        IQueryable<ApplicationUserEntity> query,
        UsersSearchAndFilterParametersDto parameters)
    {
        if (!string.IsNullOrWhiteSpace(parameters.TenantId))
            query = query.Where(u => u.TenantId == parameters.TenantId.Trim());

        if (parameters.Department.HasValue)
            query = query.Where(u => u.Department == parameters.Department.Value);

        if (!string.IsNullOrWhiteSpace(parameters.NameContains))
        {
            var term = parameters.NameContains.Trim();
            query = query.Where(u => u.FullName.Contains(term));
        }

        if (parameters.DateOfBirth.HasValue)
            query = query.Where(u => u.DateOfBirth >= parameters.DateOfBirth.Value);

        if (parameters.LastLoginAfter.HasValue)
            query = query.Where(u => u.LastLoginAt >= parameters.LastLoginAfter.Value);

        if (parameters.CreatedAfter.HasValue)
            query = query.Where(u => u.CreatedAt >= parameters.CreatedAfter.Value);

        if (parameters.CreatedBefore.HasValue)
            query = query.Where(u => u.CreatedAt <= parameters.CreatedBefore.Value);

        if (!string.IsNullOrWhiteSpace(parameters.PhoneNumber))
        {
            var phone = parameters.PhoneNumber.Trim();
            // انتِ بتقصدي PhoneNumber تبع IdentityUser (مش ContactInfo)
            query = query.Where(u => u.PhoneNumber == phone);
        }

        return query;
    }
}