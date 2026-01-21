using Application.Common;
using Application.Features.Tenants.DTOs;
using Domain.Entities;

namespace Application.Interfaces;
public interface ITenantRepository
{
    Task AddTenantAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task<Tenant?> GetByIdAsync(string tenantId);
    Task<Tenant?> GetTenantByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Tenant?> GetByDomainAsync(string domain, CancellationToken cancellationToken = default);
    Task<bool> IsTenantNameUniqueAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> IsTenantDomainUniqueAsync(string domain, CancellationToken cancellationToken = default);
    Task<bool> RemoveTenantAsync(string id);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<PagedList<Tenant, DateTime?>> GetTenantsAsync(
         TenantsSearchAndFilterParametersDto parameters,
         CancellationToken cancellationToken = default);
}