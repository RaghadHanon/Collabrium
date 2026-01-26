using Application.Common;
using Application.Features.Tenants.DTOs;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class TenantRepository : ITenantRepository
{
    private readonly AppDbContext _dbContext;

    public TenantRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        if (tenant is null) throw new ArgumentNullException(nameof(tenant));

        var entity = TenantMapper.ToEntity(tenant);
        await _dbContext.Tenants.AddAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        if (tenant is null) throw new ArgumentNullException(nameof(tenant));

        var entity = await _dbContext.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenant.Id, cancellationToken);

        if (entity is null)
            throw new InvalidOperationException($"Tenant '{tenant.Id}' was not found.");

        TenantMapper.UpdateEntity(tenant, entity);
    }

    public async Task<bool> RemoveAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) return false;

        var id = tenantId.Trim();

        var entity = await _dbContext.Tenants
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null) return false;

        _dbContext.Tenants.Remove(entity);
        return true;
    }

    public async Task<bool> UpdateProfileImageAsync(string tenantId, string url, string publicId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) return false;
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));
        if (string.IsNullOrWhiteSpace(publicId)) throw new ArgumentNullException(nameof(publicId));

        var id = tenantId.Trim();

        var entity = await _dbContext.Tenants
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (entity is null) return false;

        entity.ProfileImage = new StoredImageOwned
        {
            Url = url.Trim(),
            PublicId = publicId.Trim()
        };

        entity.UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public async Task<bool?> ToggleActivationAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) return null;

        var id = tenantId.Trim();

        var entity = await _dbContext.Tenants
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (entity is null) return null;

        entity.IsActive = !entity.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        return entity.IsActive;
    }

    public async Task<Tenant?> GetByIdAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) return null;

        var id = tenantId.Trim();

        var entity = await _dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        return entity is null ? null : TenantMapper.ToDomain(entity);
    }

    public async Task<Tenant?> GetByDomainAsync(string domain, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(domain)) return null;

        var normalized = domain.Trim().ToLowerInvariant();

        var entity = await _dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Domain == normalized, cancellationToken);

        return entity is null ? null : TenantMapper.ToDomain(entity);
    }

    public async Task<PagedList<Tenant, DateTime?>> GetPagedAsync(
        TenantsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default)
    {
        parameters ??= new TenantsSearchAndFilterParametersDto();

        var query = _dbContext.Tenants
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

    public async Task<bool> IsNameUniqueAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;

        var normalized = name.Trim().ToLowerInvariant();

        return !await _dbContext.Tenants
            .AsNoTracking()
            .AnyAsync(t => t.Name.ToLower() == normalized, cancellationToken);
    }

    public async Task<bool> IsDomainUniqueAsync(string domain, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(domain)) return false;

        var normalized = domain.Trim().ToLowerInvariant();

        return !await _dbContext.Tenants
            .AsNoTracking()
            .AnyAsync(t => t.Domain == normalized, cancellationToken);
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
            var term = parameters.DomainContains.Trim();
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
            query = query.Where(t => t.ContactInfo.PhoneNumber == phone);
        }

        return query;
    }
}