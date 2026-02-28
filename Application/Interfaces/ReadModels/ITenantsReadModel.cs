using Application.Common;
using Application.Features.Tenants.DTOs;

namespace Application.Interfaces.ReadModels;

public interface ITenantsReadModel
{
    Task<TenantDto?> GetByIdAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<TenantDto?> GetByDomainAsync(string domain, CancellationToken cancellationToken = default);

    Task<TenantWithAdminDto?> GetWithAdminAsync(string tenantId, CancellationToken cancellationToken = default);

    Task<PagedList<TenantWithAdminDto, DateTime?>> GetTenantsAsync(
        TenantsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default);
}
