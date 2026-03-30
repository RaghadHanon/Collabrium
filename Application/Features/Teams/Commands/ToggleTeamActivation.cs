using Application.Common;
using Application.Events;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
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
        ITeamsRepository teamsRepository,
        IUnitOfWork unitOfWork,
        ITeamsReadModel teamsReadModel,
        IMediator mediator,
        ICurrentUserAccessor currentUserAccessor
        ) : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenant = tenantContext.CurrentTenant;
            if (tenant is null)
                return Result<bool>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var currentUser = await currentUserAccessor.GetCurrentUserAsync();
            if (currentUser == null)
                return Result<bool>.Failure("Current user is not found.", HttpStatusCode.NotFound);

            var team = await teamsRepository.GetByIdAsync(tenant.Id, request.TeamId, cancellationToken);
            if (team is null)
                return Result<bool>.Failure("Team not found in this tenant.", HttpStatusCode.NotFound);

            if (team.IsActive)
                team.Deactivate();
            else team.Activate();

            await teamsRepository.UpdateAsync(team);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await mediator.Publish(new ToggleTeamActivationEvent(
                        team.Id,
                        await teamsReadModel.GetTeamMembersSnapshotAsync(tenant.Id, team.Id, cancellationToken),
                        currentUser.FullName,
                        team.IsActive,
                        team.Name
                    ), cancellationToken);

            return Result<bool>.Success(team.IsActive, HttpStatusCode.OK);
        }
    }
}
