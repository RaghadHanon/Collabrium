using Domain.Models;

namespace Application.Interfaces;

public interface ITenantContext
{
    string? TenantId { get; set; }
    Tenant? CurrentTenant { get; set; }
    Task<Tenant?> GetGlobalTenant(CancellationToken cancellationToken = default);
}
