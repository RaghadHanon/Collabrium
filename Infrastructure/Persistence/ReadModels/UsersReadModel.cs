using Application.Common;
using Application.Features.Users.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ReadModels;

public sealed class UsersReadModel : IUsersReadModel
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public UsersReadModel(AppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
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
            query = query.Where(u => u.CreatedAt >= parameters.Cursor.Value);

        var items = await query
            .OrderBy(u => u.CreatedAt)
            .Take(pageSize + 1)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;
        if (items.Count > pageSize)
        {
            nextCursor = items.Last().CreatedAt;
            items.RemoveAt(items.Count - 1);
        }

        return new PagedList<UserDto, DateTime?>
        {
            Items = items,
            NextCursor = nextCursor
        };
    }

    public async Task<List<UserDto>> GetUsersByTenantAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            return new List<UserDto>();

        var id = tenantId.Trim();

        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.TenantId == id)
            .OrderBy(u => u.CreatedAt)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<ApplicationUserEntity> ApplyFilters(
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

        return query;
    }
}