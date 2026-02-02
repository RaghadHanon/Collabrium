using Application.Common;
using Application.Features.Tenants.DTOs;
using Domain.Models;

namespace Application.Interfaces.Repositories;

public interface ITenantRepository
{
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<Tenant?> GetByIdAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<Tenant?> GetByDomainAsync(string domain, CancellationToken cancellationToken = default);

    Task<PagedList<Tenant, DateTime?>> GetPagedAsync(
        TenantsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default);

    Task<bool> IsNameUniqueAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> IsDomainUniqueAsync(string domain, CancellationToken cancellationToken = default);
}