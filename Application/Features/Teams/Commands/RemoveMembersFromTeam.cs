using Application.Common;
using Application.Events;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Teams.Commands;

public sealed class RemoveMembersFromTeam
{
    public sealed class Command : IRequest<Result<string>>
    {
        public required string TeamId { get; init; }
        public required string[] MembersIds { get; init; } = Array.Empty<string>();
    }

    public sealed class Handler(
        ITeamsRepository teamsRepository,
        ITeamsReadModel teamsReadModel,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ICurrentUserAccessor currentUserAccessor
    ) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<string>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var currentUser = await currentUserAccessor.GetCurrentUserAsync();
            if (currentUser == null)
                return Result<string>.Failure("Current user is not found.", HttpStatusCode.NotFound);

            var team = await teamsReadModel.GetByIdAsync(tenantId, request.TeamId, cancellationToken);
            if (team is null)
                return Result<string>.Failure("Team is not found.", HttpStatusCode.BadRequest);

            if (!team.IsActive)
                return Result<string>.Failure("Can't remove members from inactive team.", HttpStatusCode.BadRequest);

            if (request.MembersIds is null || request.MembersIds.Length == 0)
                return Result<string>.Failure("MembersIds is required.", HttpStatusCode.BadRequest);

            var removedMembers = (await teamsReadModel.GetTeamMembersSnapshotAsync(tenantId, request.TeamId, cancellationToken))
                        .Where(tm => request.MembersIds.Contains(tm.Id))
                        .ToList();

            await unitOfWork.ExecuteInTransactionAsync(async cancellationToken =>
            {
                await teamsRepository.RemoveMembersAsync(
                    tenantId: tenantId,
                    teamId: request.TeamId,
                    memberIds: request.MembersIds,
                    cancellationToken: cancellationToken);
            }, cancellationToken);

            await mediator.Publish(new RemoveMemberFromTeamEvent(
                     tenantId,
                     team.Id,
                     removedMembers,
                     currentUser.Id,
                     currentUser.FullName,
                     team.TeamLeadName,
                     team.Name
                 ), cancellationToken);

            return Result<string>.Success("Members removed successfully.", HttpStatusCode.OK);
        }
    }
}
