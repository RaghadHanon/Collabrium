using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Teams.Commands;
public class ToggleTeamActivation
{
    public class Command : IRequest<Result<bool>>
    {
        public required string TeamId { get; set; }
    }

    public class Handler(
        ITenantContext tenantContext,
        ITeamsRepository teamsRepository
        ) : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenant = tenantContext.CurrentTenant;
            if (tenant is null)
                return Result<bool>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            await teamsRepository.ToggleActivationAsync(tenant.Id, request.TeamId, cancellationToken);
            await teamsRepository.SaveChangesAsync(cancellationToken);

            var updated = await teamsRepository.GetByIdAsync(tenant.Id, request.TeamId, cancellationToken);
            return updated is null
                ? Result<bool>.Failure("Team not found in this tenant.", HttpStatusCode.NotFound)
                : Result<bool>.Success(updated.IsActive, HttpStatusCode.OK);
        }
    }
}
