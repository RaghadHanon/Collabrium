using Application.Common;
using Application.Features.Tenants.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ReadModels;

public sealed class TenantsReadModel : ITenantsReadModel
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public TenantsReadModel(AppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<TenantDto?> GetByIdAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            return null;

        var id = tenantId.Trim();

        return await _dbContext.Tenants
            .AsNoTracking()
            .Where(t => t.Id == id)
            .ProjectTo<TenantDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TenantDto?> GetByDomainAsync(
        string domain,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(domain))
            return null;

        var normalized = domain.Trim().ToLowerInvariant();

        return await _dbContext.Tenants
            .AsNoTracking()
            .Where(t => t.Domain == normalized)
            .ProjectTo<TenantDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TenantWithAdminDto?> GetWithAdminAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            return null;

        var id = tenantId.Trim();

        return await _dbContext.Tenants
            .AsNoTracking()
            .Include(t => t.TenantAdmin)
            .Where(t => t.Id == id)
            .ProjectTo<TenantWithAdminDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedList<TenantWithAdminDto, DateTime?>> GetTenantsAsync(
        TenantsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default)
    {
        parameters ??= new TenantsSearchAndFilterParametersDto();
        var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

        var query = _dbContext.Tenants
                .AsNoTracking();

        query = ApplyFilters(query, parameters);

        if (parameters.Cursor.HasValue)
            query = query.Where(t => t.CreatedAt >= parameters.Cursor.Value);

        var items = await query
            .OrderBy(t => t.CreatedAt)
            .Take(pageSize + 1)
            .ProjectTo<TenantWithAdminDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;
        if (items.Count > pageSize)
        {
            nextCursor = items.Last().CreatedAt;
            items.RemoveAt(items.Count - 1);
        }

        return new PagedList<TenantWithAdminDto, DateTime?>
        {
            Items = items,
            NextCursor = nextCursor
        };
    }

    private static IQueryable<TenantEntity> ApplyFilters(
        IQueryable<TenantEntity> query,
        TenantsSearchAndFilterParametersDto parameters)
    {
        if (!string.IsNullOrWhiteSpace(parameters.Country))
        {
            var country = parameters.Country.Trim().ToLowerInvariant();
            query = query.Where(t => t.Country != null && t.Country.ToLower() == country);
        }

        if (!string.IsNullOrWhiteSpace(parameters.DescriptionContains))
        {
            var term = parameters.DescriptionContains.Trim();
            query = query.Where(t => t.Description != null && t.Description.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(parameters.NameContains))
        {
            var term = parameters.NameContains.Trim();
            query = query.Where(t => t.Name.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(parameters.DomainContains))
        {
            var term = parameters.DomainContains.Trim().ToLowerInvariant();
            query = query.Where(t => t.Domain.Contains(term));
        }

        if (parameters.IsActive.HasValue)
            query = query.Where(t => t.IsActive == parameters.IsActive.Value);

        if (parameters.CreatedAfter.HasValue)
            query = query.Where(t => t.CreatedAt >= parameters.CreatedAfter.Value);

        if (parameters.CreatedBefore.HasValue)
            query = query.Where(t => t.CreatedAt <= parameters.CreatedBefore.Value);

        if (!string.IsNullOrWhiteSpace(parameters.PhoneNumber))
        {
            var phone = parameters.PhoneNumber.Trim();
            query = query.Where(t => t.ContactDetails.PhoneNumber == phone);
        }

        return query;
    }
}