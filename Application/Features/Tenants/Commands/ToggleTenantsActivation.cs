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

    public class Handler(ITenantRepository tenantRepository, ITenantContext tenantContext)
        : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = request.TenantId ?? tenantContext.CurrentTenant?.Id;

            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<bool>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var newState = await tenantRepository.ToggleActivationAsync(tenantId, cancellationToken);
            if (newState == null)
                return Result<bool>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            await tenantRepository.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(newState.Value, HttpStatusCode.OK);
        }
    }
}