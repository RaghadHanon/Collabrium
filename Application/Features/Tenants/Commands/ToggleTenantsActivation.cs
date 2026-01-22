using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Tenants.Commands;
public class ToggleTenantsActivation
{
    public class Command : IRequest<Result<bool>>
    {
        public string? TenantId { get; set; }
    }

    public class Handler(ITenantRepository _tenantRepository, ITenantContext tenantContext)
        : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenant = request.TenantId == null ? tenantContext.CurrentTenant 
                : await _tenantRepository.GetByIdAsync(request.TenantId!);
            if (tenant == null)
                return Result<bool>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            tenant.IsActive = !tenant.IsActive;
            await _tenantRepository.SaveChangesAsync();

            return Result<bool>.Success(tenant.IsActive, HttpStatusCode.OK);
        }
    }
}
