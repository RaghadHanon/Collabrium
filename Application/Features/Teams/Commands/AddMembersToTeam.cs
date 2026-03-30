using Application.Common;
using Application.Events;
using Application.Features.Sprints.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.Teams.Commands;

public sealed class AddMembersToTeam
{
    public sealed class Command : IRequest<Result<string>>
    {
        public required string TeamId { get; init; }
        public required string[] MembersIds { get; init; } = Array.Empty<string>();
    }

    public sealed class Handler(
        ITeamsRepository teamsRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ITeamsReadModel teamsReadModel,
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

            if (string.IsNullOrWhiteSpace(request.TeamId))
                return Result<string>.Failure("TeamId is required.", HttpStatusCode.BadRequest);

            if (request.MembersIds is null || request.MembersIds.Length == 0)
                return Result<string>.Failure("MembersIds is required.", HttpStatusCode.BadRequest);

            try
            {
                await teamsRepository.AddMembersAsync(
                    tenantId: tenantId,
                    teamId: request.TeamId,
                    memberIds: request.MembersIds,
                    cancellationToken: cancellationToken);

                await unitOfWork.SaveChangesAsync(cancellationToken);

                var team = await teamsReadModel.GetByIdAsync(tenantId, request.TeamId, cancellationToken);  

                await mediator.Publish(new AddMemberToTeamEvent(
                        await teamsReadModel.GetTeamMembersSnapshotAsync(tenantId, request.TeamId, cancellationToken),
                        currentUser.FullName,
                        team!.TeamLeadName,
                        team!.Name
                    ), cancellationToken);

                return Result<string>.Success("Members added successfully.", HttpStatusCode.OK);
            }
            catch (InvalidOperationException ex)
            {
                return Result<string>.Failure(ex.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}