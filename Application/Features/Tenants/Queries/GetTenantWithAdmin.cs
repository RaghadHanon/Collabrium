using Application.Common;
using Application.Features.Tenants.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Tenants.Queries;

public class GetTenantWithAdmin
{
    public class Query : IRequest<Result<TenantWithAdminDto>>
    {
        public string? TenantId { get; set; }
    }

    public class Handler(
        ITenantsReadModel tenantsReadModel,
        ITenantContext tenantContext
        ) : IRequestHandler<Query, Result<TenantWithAdminDto>>
    {
        public async Task<Result<TenantWithAdminDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = request.TenantId ?? tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<TenantWithAdminDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var dto = await tenantsReadModel.GetWithAdminAsync(tenantId, cancellationToken);

            return dto is null
                ? Result<TenantWithAdminDto>.Failure("Tenant not found.", HttpStatusCode.NotFound)
                : Result<TenantWithAdminDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}