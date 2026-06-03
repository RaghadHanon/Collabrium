using Application.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.TenantContexts;

public sealed class TenantContext : ITenantContext
{
    private const string GlobalTenantDomain = "global.platform";
    private readonly IServiceScopeFactory _scopeFactory;

    public TenantContext(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public string? TenantId { get; set; }
    public Tenant? CurrentTenant { get; set; }

    public async Task<Tenant?> GetGlobalTenant(CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var tenantEntity = await dbContext.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                tenant => tenant.Domain == GlobalTenantDomain,
                cancellationToken);

        return tenantEntity is null
            ? null
            : TenantMapper.ToDomain(tenantEntity);
    }
}