using Application.Common;
using Application.Features.Tenants.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using MediatR;
using System.Net;

namespace Application.Features.Tenants.Queries;

public sealed class GetTenant
{
    public sealed class Query : IRequest<Result<TenantWithAdminDto>>
    {
        public string? TenantId { get; init; }
    }

    public sealed class Handler : IRequestHandler<Query, Result<TenantWithAdminDto>>
    {
        private readonly ITenantsReadModel _tenantsReadModel;
        private readonly ITenantContext _tenantContext;

        public Handler(ITenantsReadModel tenantsReadModel, ITenantContext tenantContext)
        {
            _tenantsReadModel = tenantsReadModel ?? throw new ArgumentNullException(nameof(tenantsReadModel));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        public async Task<Result<TenantWithAdminDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = request.TenantId ?? _tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<TenantWithAdminDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var dto = await _tenantsReadModel.GetWithAdminAsync(tenantId, cancellationToken);

            if (dto is null)
                return Result<TenantWithAdminDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            return Result<TenantWithAdminDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}