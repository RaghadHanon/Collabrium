using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Tenants.Commands;

public class ToggleTenantActiveStatus
{
    public class Command : IRequest<Result<bool>>
    {
        public string? TenantId { get; set; }
        public bool IsSuper { get; set; }
    }

    public class Handler(
        ITenantRepository tenantRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork
        ) : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = request.TenantId ?? tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<bool>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var tenant = await tenantRepository.GetByIdAsync(tenantId, cancellationToken);
            if (tenant is null)
                return Result<bool>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            if(tenant.IsActive)
                tenant.Deactivate();
            else if(request.IsSuper)
                tenant.Activate();

            await tenantRepository.UpdateAsync(tenant, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(tenant.IsActive, HttpStatusCode.OK);
        }
    }
}