using Application.Interfaces;
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
        return await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
    }

    public async Task<Tenant?> GetTenantByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }
    public async Task<Tenant?> GetByDomainAsync(string domain, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tenants
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
}