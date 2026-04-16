using Application.Common;
using Application.Features.Users.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

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
    public async Task<PagedList<AssignableUserDto, DateTime?>> GetAssignableUsersAsync(
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
    private async Task<PagedList<AssignableUserDto, DateTime?>> ApplyAssignableUsersPagination(
     AssignableUsersSearchAndFilterParametersDto parameters,
     int pageSize,
     IQueryable<ApplicationUserEntity> query,
     CancellationToken cancellationToken)
    {
        if (parameters.Cursor.HasValue)
            query = query.Where(u => u.CreatedAt > parameters.Cursor.Value);

        var items = await query
            .OrderBy(u => u.CreatedAt)
            .Select(u => new
            {
                Cursor = u.CreatedAt,
                Item = new AssignableUserDto
                {
                    Id = u.Id,
                    FullName = u.FullName
                }
            })
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;
        var hasMore = items.Count > pageSize;

        if (hasMore)
        {
            items.RemoveAt(items.Count - 1);
            nextCursor = items[^1].Cursor;
        }

        return new PagedList<AssignableUserDto, DateTime?>
        {
            Items = items.Select(x => x.Item).ToList(),
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

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var term = parameters.Search.Trim();
            query = query.Where(u => u.FullName.Contains(term));
        }
        var blockedJobTitles = new[]
        {
        "ProductOwner",
        "TeamLead",
        "TenantAdmin",
        "BusinessAnalyst"
    };

        query = query.Where(u => !blockedJobTitles.Contains(u.JobTitle));

        return query;
    }
}