using Application.Common;
using Application.Features.Tags.DTOs;
using Application.Features.Users.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Enums;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

namespace Infrastructure.Persistence.ReadModels;

public class UsersReadModel : IUsersReadModel
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public UsersReadModel(AppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<UserWithTenantDto?> GetUserWithTenantAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        var id = userId.Trim();

        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .ProjectTo<UserWithTenantDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserDto?> GetUserByIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;
        var id = userId.Trim();
        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<string?> GetUserDisplayNameAsync(
    string tenantId,
    string userId,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(userId))
            return null;

        var tId = tenantId.Trim();
        var uId = userId.Trim();

        return await _dbContext.Users
            .AsNoTracking()
            .Where(user =>
                user.Id == uId &&
                user.TenantId == tId)
            .Select(user => user.FullName)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> UserExistsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        var id = userId.Trim();
        return await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<PagedList<UserDto, DateTime?>> GetUsersAsync(
        UsersSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default)
    {
        parameters ??= new UsersSearchAndFilterParametersDto();
        var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

        var query = _dbContext.Users.AsNoTracking();

        query = ApplyFilters(query, parameters);
        return await ApplyPagination(parameters, pageSize, query, cancellationToken);
    }
    public async Task<UserSnapshotDto?> GetUserSnapshotByIdAsync(
    string userId,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        var id = userId.Trim();

        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
             .ProjectTo<UserSnapshotDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<TagDto>> GetUserSkillsAsync(
    string userId,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return new();

        var id = userId.Trim();

        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .SelectMany(u => u.Skills)
            .Select(s => new TagDto
            {
                Id = s.Id,
                Name = s.Name
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<PagedList<UserDto, DateTime?>> ApplyPagination(
        UsersSearchAndFilterParametersDto parameters, int pageSize,
        IQueryable<ApplicationUserEntity> query,
        CancellationToken cancellationToken)
    {
        if (parameters.Cursor.HasValue)
            query = query.Where(u => u.CreatedAt > parameters.Cursor.Value);

        var items = await query
            .OrderBy(u => u.CreatedAt)
            .Take(pageSize + 1)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;
        var hasMore = items.Count > pageSize;

        if (hasMore)
        {
            items.RemoveAt(items.Count - 1);
            nextCursor = items[^1].CreatedAt;
        }

        return new PagedList<UserDto, DateTime?>
        {
            Items = items,
            NextCursor = nextCursor
        };
    }

    private IQueryable<ApplicationUserEntity> ApplyFilters(
        IQueryable<ApplicationUserEntity> query,
        UsersSearchAndFilterParametersDto parameters)
    {
        if (!string.IsNullOrWhiteSpace(parameters.TenantId))
        {
            var tenantId = parameters.TenantId.Trim();
            query = query.Where(u => u.TenantId == tenantId);
        }

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

            query = query.Where(u => u.ContactDetails.PhoneNumber == phone);
        }

        if (!string.IsNullOrWhiteSpace(parameters.JobTitle.ToString()))
        {
            query = query.Where(u => u.JobTitle == parameters.JobTitle.ToString());
        }

        return query;
    }
    public async Task<PagedList<UserDto, DateTime?>> GetAssignableUsersAsync(
    AssignableUsersSearchAndFilterParametersDto parameters,
    CancellationToken cancellationToken = default)
    {
        parameters ??= new AssignableUsersSearchAndFilterParametersDto();
        var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

        var query = _dbContext.Users.AsNoTracking();

        query = ApplyAssignableUsersFilters(query, parameters);

        return await ApplyAssignableUsersPagination(
            parameters,
            pageSize,
            query,
            cancellationToken);
    }
    private async Task<PagedList<UserDto, DateTime?>> ApplyAssignableUsersPagination(
     AssignableUsersSearchAndFilterParametersDto parameters,
     int pageSize,
     IQueryable<ApplicationUserEntity> query,
     CancellationToken cancellationToken)
    {
        if (parameters.Cursor.HasValue)
            query = query.Where(u => u.CreatedAt > parameters.Cursor.Value);


        var items = await query
            .OrderBy(u => u.CreatedAt)
            .Take(pageSize + 1)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;
        var hasMore = items.Count > pageSize;

        if (hasMore)
        {
            items.RemoveAt(items.Count - 1);
            nextCursor = items[^1].CreatedAt;
        }

        return new PagedList<UserDto, DateTime?>
        {
            Items = items,
            NextCursor = nextCursor
        };
    }
    private IQueryable<ApplicationUserEntity> ApplyAssignableUsersFilters(
     IQueryable<ApplicationUserEntity> query,
     AssignableUsersSearchAndFilterParametersDto parameters)
    {
        if (!string.IsNullOrWhiteSpace(parameters.TenantId))
        {
            var tenantId = parameters.TenantId.Trim();
            query = query.Where(u => u.TenantId == tenantId);
        }

        if (!string.IsNullOrWhiteSpace(parameters.CurrentUserId))
        {
            var currentUserId = parameters.CurrentUserId.Trim();
            query = query.Where(u => u.Id != currentUserId);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var term = parameters.Search.Trim();
            query = query.Where(u => u.FullName.Contains(term));
        }
        var blockedJobTitles = CourseAssignmentRules.GetBlockedJobTitles();

        query = query.Where(u => !blockedJobTitles.Contains(u.JobTitle));
        return query;
    }
    public async Task<List<string>> GetTenantUserIdsAsync(
    string tenantId,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            return new();

        var tId = tenantId.Trim();

        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.TenantId == tId)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<string>> GetUserIdsByRoleAsync(
        string tenantId,
        string roleName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(roleName))
            return new();

        var tId = tenantId.Trim();
        var role = roleName.Trim();

        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.TenantId == tId && u.JobTitle == role)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);
    }
    public async Task<List<UserDto>> GetManagersAsync(
    string tenantId,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            return new();

        var tId = tenantId.Trim();

        var managerRoles = new[]
        {
        nameof(UserRole.TenantAdmin),
        nameof(UserRole.CEO),
        nameof(UserRole.ProductOwner),
        nameof(UserRole.TeamLead),
        nameof(UserRole.BusinessAnalyst)
    };

        return await (
            from user in _dbContext.Users.AsNoTracking()
            join userRole in _dbContext.UserRoles.AsNoTracking()
                on user.Id equals userRole.UserId
            join role in _dbContext.Roles.AsNoTracking()
                on userRole.RoleId equals role.Id
            where user.TenantId == tId
                  && role.Name != null
                  && managerRoles.Contains(role.Name)
            select user
        )
        .Distinct()
        .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
        .ToListAsync(cancellationToken);
    }
}