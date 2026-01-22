using Application.Common;
using Application.Features.Tenants.DTOs;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;
public class TenantRepository : ITenantRepository
{
    private readonly AppDbContext _dbContext;

    public TenantRepository(AppDbContext context)
    {
        _dbContext = context;
    }

    public async Task AddTenantAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await _dbContext.Tenants.AddAsync(tenant, cancellationToken);
    }

    public async Task<Tenant?> GetByIdAsync(string tenantId)
    {
        return await _dbContext.Tenants
            .Include(t => t.TenantContactInfo)
            .Include(tenantId => tenantId.ProfileImage)
            .FirstOrDefaultAsync(t => t.Id == tenantId);
    }

    public async Task<Tenant?> GetTenantByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tenants
            .Include(t => t.TenantContactInfo)
            .Include(tenantId => tenantId.ProfileImage)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Tenant?> GetByDomainAsync(string domain, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tenants
            .Include(t => t.TenantContactInfo)
            .Include(tenantId => tenantId.ProfileImage)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Domain == domain, cancellationToken);
    }

    public async Task<bool> IsTenantNameUniqueAsync(string name, CancellationToken cancellationToken = default)
    {
        var nameExists = await _dbContext.Tenants
            .AnyAsync(t => t.Name.ToLower() == name.ToLower(), cancellationToken);

        return !nameExists;
    }
    public async Task<bool> IsTenantDomainUniqueAsync(string domain, CancellationToken cancellationToken = default)
    {
        var domainExists = await _dbContext.Tenants
            .AnyAsync(t => t.Domain.ToLower() == domain.ToLower(), cancellationToken);

        return !domainExists;
    }

    public async Task<bool> RemoveTenantAsync(string id)
    {
        var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(x => x.Id == id);

        if (tenant == null)
            return false;

        _dbContext.Tenants.Remove(tenant);

        return true;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedList<Tenant, DateTime?>> GetTenantsAsync(
         TenantsSearchAndFilterParametersDto parameters,
         CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Tenants
            .Include(t => t.TenantContactInfo)
            .Include(u => u.ProfileImage)
            .AsNoTracking()
            .AsQueryable();

        query = ApplyFilters(query, parameters);
        return await ApplyPagination(parameters, query, cancellationToken);
    }

    private static async Task<PagedList<Tenant, DateTime?>> ApplyPagination(
        TenantsSearchAndFilterParametersDto parameters, IQueryable<Tenant> query,
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

        return new PagedList<Tenant, DateTime?>
        {
            Items = items,
            NextCursor = nextCursor
        };
    }

    private static IQueryable<Tenant> ApplyFilters(
        IQueryable<Tenant> query,
        TenantsSearchAndFilterParametersDto parameters)
    {
        if (!string.IsNullOrWhiteSpace(parameters.Country))
            query = query.Where(c => !string.IsNullOrWhiteSpace(c.Country) &&
            c.Country.ToLower() == parameters.Country.ToLower());

        if (!string.IsNullOrWhiteSpace(parameters.DescriptionContains))
            query = query.Where(c => !string.IsNullOrWhiteSpace(c.Description) &&
            c.Description.Contains(parameters.DescriptionContains));

        if (!string.IsNullOrWhiteSpace(parameters.NameContains))
            query = query.Where(c => c.Name.Contains(parameters.NameContains));

        if (!string.IsNullOrWhiteSpace(parameters.DomainContains))
            query = query.Where(c => c.Domain.Contains(parameters.DomainContains));

        if (parameters.IsActive.HasValue)
            query = query.Where(c => c.IsActive == parameters.IsActive.Value);

        if (parameters.CreatedAfter.HasValue)
            query = query.Where(c => c.CreatedAt >= parameters.CreatedAfter.Value);

        if (parameters.CreatedBefore.HasValue)
            query = query.Where(c => c.CreatedAt <= parameters.CreatedBefore.Value);

        if (!string.IsNullOrWhiteSpace(parameters.PhoneNumber))
            query = query.Where(c => c.TenantContactInfo != null &&
          c.TenantContactInfo.PhoneNumber == parameters.PhoneNumber);

        return query;
    }
}