using Application.Common;
using Application.Features.Tenants.DTOs;
using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly AppDbContext _dbContext;

    public TenantRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tenant);

        _dbContext.Tenants.Add(TenantMapper.ToEntity(tenant));
        return Task.CompletedTask;
    }

    public async Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tenant);

        var entity = await _dbContext.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenant.Id, cancellationToken);

        if (entity is null)
            throw new InvalidOperationException($"Tenant '{tenant.Id}' was not found.");

        TenantMapper.UpdateEntity(tenant, entity);
    }

    public async Task<bool> RemoveAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        if (!TryNormalize(tenantId, out var id))
            return false;

        var entity = await _dbContext.Tenants
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null) return false;

        _dbContext.Tenants.Remove(entity);
        return true;
    }

    public async Task<Tenant?> GetByIdAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        if (!TryNormalize(tenantId, out var id))
            return null;

        var entity = await _dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        return entity is null ? null : TenantMapper.ToDomain(entity);
    }

    public async Task<bool> IsExistAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        if (!TryNormalize(tenantId, out var id))
            return false;

        return await _dbContext.Tenants
            .AnyAsync(t => t.Id ==  tenantId, cancellationToken);   
    }

    public async Task<Tenant?> GetByDomainAsync(string domain, CancellationToken cancellationToken = default)
    {
        if (!TryNormalize(domain, out var normalized))
            return null;

        normalized = normalized.ToLowerInvariant();

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
        var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

        var query = _dbContext.Tenants.AsNoTracking();
        query = ApplyFilters(query, parameters);

        if (parameters.Cursor.HasValue)
            query = query.Where(t => t.CreatedAt > parameters.Cursor.Value);

        var entities = await query
            .OrderBy(t => t.CreatedAt)
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;
        if (entities.Count > pageSize)
        {
            nextCursor = entities[^1].CreatedAt;
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
        if (!TryNormalize(name, out var normalized))
            return false;

        normalized = normalized.ToLowerInvariant();

        return !await _dbContext.Tenants
            .AsNoTracking()
            .AnyAsync(t => t.Name.ToLower() == normalized, cancellationToken);
    }

    public async Task<bool> IsDomainUniqueAsync(string domain, CancellationToken cancellationToken = default)
    {
        if (!TryNormalize(domain, out var normalized))
            return false;

        normalized = normalized.ToLowerInvariant();

        return !await _dbContext.Tenants
            .AsNoTracking()
            .AnyAsync(t => t.Domain == normalized, cancellationToken);
    }

    private static bool TryNormalize(string? value, out string normalized)
    {
        normalized = value?.Trim() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(normalized);
    }

    private static IQueryable<TenantEntity> ApplyFilters(IQueryable<TenantEntity> query, TenantsSearchAndFilterParametersDto p)
    {
        if (!string.IsNullOrWhiteSpace(p.Country))
        {
            var country = p.Country.Trim().ToLowerInvariant();
            query = query.Where(t => t.Country != null && t.Country.ToLower() == country);
        }

        if (!string.IsNullOrWhiteSpace(p.DescriptionContains))
        {
            var term = p.DescriptionContains.Trim();
            query = query.Where(t => t.Description != null && t.Description.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(p.NameContains))
        {
            var term = p.NameContains.Trim();
            query = query.Where(t => t.Name.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(p.DomainContains))
        {
            var term = p.DomainContains.Trim().ToLowerInvariant();
            query = query.Where(t => t.Domain.Contains(term));
        }

        if (p.IsActive.HasValue)
            query = query.Where(t => t.IsActive == p.IsActive.Value);

        if (p.CreatedAfter.HasValue)
            query = query.Where(t => t.CreatedAt >= p.CreatedAfter.Value);

        if (p.CreatedBefore.HasValue)
            query = query.Where(t => t.CreatedAt <= p.CreatedBefore.Value);

        if (!string.IsNullOrWhiteSpace(p.PhoneNumber))
        {
            var phone = p.PhoneNumber.Trim();
            query = query.Where(t => t.ContactDetails.PhoneNumber == phone);
        }

        return query;
    }
}