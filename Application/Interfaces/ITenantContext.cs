using Domain.Entities;

namespace Application.Interfaces;
public interface ITenantContext
{
    Tenant? CurrentTenant { get; set; }
    string? TenantId { get; set; }
    Task<Tenant?> GetGlobalTenant();
}