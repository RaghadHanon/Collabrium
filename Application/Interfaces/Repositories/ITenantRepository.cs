using Application.Common;
using Application.Features.Tenants.DTOs;
using Application.Features.Users.DTOs;
using Domain.Models;

namespace Application.Interfaces.Repositories;

public interface ITenantRepository
{
    Task AddTenantAsync(Tenant tenant, CancellationToken cancellationToken = default);

    Task<Tenant?> GetByIdAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<Tenant?> GetTenantByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Tenant?> GetByDomainAsync(string domain, CancellationToken cancellationToken = default);

    Task<bool> IsTenantNameUniqueAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> IsTenantDomainUniqueAsync(string domain, CancellationToken cancellationToken = default);

    Task<bool> RemoveTenantAsync(string id, CancellationToken cancellationToken = default);

    Task<PagedList<Tenant, DateTime?>> GetTenantsAsync(
        TenantsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default);
   

    Task<bool> UpdateProfileImageAsync(string tenantId, string url, string publicId, CancellationToken cancellationToken = default);
    Task<bool?> ToggleActivationAsync(string tenantId, CancellationToken cancellationToken = default);
}