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
        ITenantRepository _tenantRepository, 
        ITenantContext tenantContext,
        ITeamsRepository teamsRepository
        ) : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenant = tenantContext.CurrentTenant;
            if (tenant == null)
                return Result<bool>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var team = await teamsRepository.GetByIdAsync(tenant.Id, request.TeamId, cancellationToken);
            if (team == null || team.TenantId != tenant.Id)
                return Result<bool>.Failure("Team not found in this tenant.", HttpStatusCode.NotFound);

            team.IsActive = !team.IsActive;
            await _tenantRepository.SaveChangesAsync();

            return Result<bool>.Success(team.IsActive, HttpStatusCode.OK);
        }
    }
}
