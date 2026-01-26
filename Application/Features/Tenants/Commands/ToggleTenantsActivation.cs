using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Tenants.Commands;

public sealed class ToggleTenantsActivation
{
    public sealed class Command : IRequest<Result<bool>>
    {
        public string? TenantId { get; init; }
    }

    public sealed class Handler : IRequestHandler<Command, Result<bool>>
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly ITenantContext _tenantContext;
        private readonly IUnitOfWork _uow;

        public Handler(ITenantRepository tenantRepository, ITenantContext tenantContext, IUnitOfWork uow)
        {
            _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = request.TenantId ?? _tenantContext.CurrentTenant?.Id;

            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<bool>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var newState = await _tenantRepository.ToggleActivationAsync(tenantId, cancellationToken);

            if (newState is null)
                return Result<bool>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            await _uow.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(newState.Value, HttpStatusCode.OK);
        }
    }
}