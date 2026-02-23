using Application.Common;
using Application.Features.Tenants.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using MediatR;
using System.Net;

namespace Application.Features.Tenants.Queries;

public class GetTenant
{
    public class Query : IRequest<Result<TenantDto>>
    {
        public string? TenantId { get; set; }
    }

    public class Handler(
        ITenantsReadModel tenantsReadModel,
        ITenantContext tenantContext
        ) : IRequestHandler<Query, Result<TenantDto>>
    {
        public async Task<Result<TenantDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = request.TenantId ?? tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<TenantDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var dto = await tenantsReadModel.GetByIdAsync(tenantId, cancellationToken);

            return dto is null
                ? Result<TenantDto>.Failure("Tenant not found.", HttpStatusCode.NotFound)
                : Result<TenantDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}