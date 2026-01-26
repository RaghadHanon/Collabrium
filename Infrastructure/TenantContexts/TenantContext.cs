using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.TenantContexts;

public class TenantContext : ITenantContext
{
    private const string GlobalTenantDomain = "global.platform";
    private readonly IServiceProvider _serviceProvider;

    public TenantContext(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public string? TenantId { get; set; }
    public Tenant? CurrentTenant { get; set; }

    public async Task<Tenant?> GetGlobalTenant(CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var tenantEntity = await dbContext.Tenants
            .FirstOrDefaultAsync(t => t.Domain == GlobalTenantDomain, cancellationToken);

        return tenantEntity == null ? null : TenantMapper.ToDomain(tenantEntity);
    }
}
