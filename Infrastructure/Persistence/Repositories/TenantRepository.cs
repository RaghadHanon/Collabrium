using Application.Common;
using Application.Features.Tenants.DTOs;
using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Entities;
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
        var entity = TenantMapper.ToEntity(tenant);
        await _dbContext.Tenants.AddAsync(entity, cancellationToken);
    }

    public async Task<Tenant?> GetByIdAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Tenants
            .Include(t => t.TenantContactInfo)
            .Include(t => t.ProfileImage)
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

        return entity == null ? null : TenantMapper.ToDomain(entity);
    }

    public async Task<Tenant?> GetTenantByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Tenants
            .Include(t => t.TenantContactInfo)
            .Include(t => t.ProfileImage)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        return entity == null ? null : TenantMapper.ToDomain(entity);
    }

    public async Task<Tenant?> GetByDomainAsync(string domain, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Tenants
            .Include(t => t.TenantContactInfo)
            .Include(t => t.ProfileImage)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Domain == domain, cancellationToken);

        return entity == null ? null : TenantMapper.ToDomain(entity);
    }

    public async Task<bool> IsTenantNameUniqueAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToLower();
        var exists = await _dbContext.Tenants
            .AnyAsync(t => t.Name.ToLower() == normalized, cancellationToken);

        return !exists;
    }

    public async Task<bool> IsTenantDomainUniqueAsync(string domain, CancellationToken cancellationToken = default)
    {
        var normalized = domain.Trim().ToLower();
        var exists = await _dbContext.Tenants
            .AnyAsync(t => t.Domain.ToLower() == normalized, cancellationToken);

        return !exists;
    }

    public async Task<bool> RemoveTenantAsync(string id, CancellationToken cancellationToken = default)
    {
        var tenant = await _dbContext.Tenants
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (tenant == null) return false;

        _dbContext.Tenants.Remove(tenant);
        return true;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedList<Tenant, DateTime?>> GetTenantsAsync(
        TenantsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Tenants
            .Include(t => t.TenantContactInfo)
            .Include(t => t.ProfileImage)
            .AsNoTracking()
            .AsQueryable();

        query = ApplyFilters(query, parameters);

        if (parameters.Cursor.HasValue)
            query = query.Where(t => t.CreatedAt >= parameters.Cursor.Value);

        var entities = await query
            .OrderBy(t => t.CreatedAt)
            .Take(parameters.PageSize + 1)
            .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;
        if (entities.Count > parameters.PageSize)
        {
            nextCursor = entities.Last().CreatedAt;
            entities.RemoveAt(entities.Count - 1);
        }

        return new PagedList<Tenant, DateTime?>
        {
            Items = entities.Select(TenantMapper.ToDomain).ToList(),
            NextCursor = nextCursor
        };
    }

    private static IQueryable<TenantEntity> ApplyFilters(
        IQueryable<TenantEntity> query,
        TenantsSearchAndFilterParametersDto parameters)
    {
        if (!string.IsNullOrWhiteSpace(parameters.Country))
        {
            var country = parameters.Country.Trim().ToLower();
            query = query.Where(t => t.Country != null && t.Country.ToLower() == country);
        }

        if (!string.IsNullOrWhiteSpace(parameters.DescriptionContains))
            query = query.Where(t => t.Description != null && t.Description.Contains(parameters.DescriptionContains));

        if (!string.IsNullOrWhiteSpace(parameters.NameContains))
            query = query.Where(t => t.Name.Contains(parameters.NameContains));

        if (!string.IsNullOrWhiteSpace(parameters.DomainContains))
            query = query.Where(t => t.Domain.Contains(parameters.DomainContains));

        if (parameters.IsActive.HasValue)
            query = query.Where(t => t.IsActive == parameters.IsActive.Value);

        if (parameters.CreatedAfter.HasValue)
            query = query.Where(t => t.CreatedAt >= parameters.CreatedAfter.Value);

        if (parameters.CreatedBefore.HasValue)
            query = query.Where(t => t.CreatedAt <= parameters.CreatedBefore.Value);

        if (!string.IsNullOrWhiteSpace(parameters.PhoneNumber))
            query = query.Where(t => t.TenantContactInfo != null &&
                                     t.TenantContactInfo.PhoneNumber == parameters.PhoneNumber);

        return query;
    }

    public async Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Tenants
            .Include(t => t.TenantContactInfo)
            .Include(t => t.ProfileImage)
            .FirstOrDefaultAsync(t => t.Id == tenant.Id, cancellationToken);

        if (entity == null)
            throw new InvalidOperationException($"Tenant '{tenant.Id}' was not found.");

        TenantMapper.UpdateEntity(tenant, entity);
    }



    public async Task<bool> UpdateProfileImageAsync(string tenantId, string url, string publicId, CancellationToken cancellationToken = default) 
    { 
        var entity = await _dbContext.Tenants
            .Include(t => t.ProfileImage)
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken); 

        if (entity == null) 
            return false; 

        if (entity.ProfileImage == null) 
            entity.ProfileImage = new TenantImageEntity { TenantId = entity.Id }; 
        
        entity.ProfileImage.Url = url; 
        entity.ProfileImage.PublicId = publicId; 
        entity.ProfileImage.TenantId = entity.Id;

        return true; 
    }
    public async Task<bool?> ToggleActivationAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Tenants
            .Include(t => t.TenantContactInfo)
            .Include(t => t.ProfileImage)
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken); 
        
        if (entity == null) 
            return null; 
        
        var domain = TenantMapper.ToDomain(entity); 
        if (domain.IsActive) 
            domain.Deactivate(); 
        else domain.Activate(); 
        
        TenantMapper.UpdateEntity(domain, entity);
        
        return entity.IsActive;
    }
}
