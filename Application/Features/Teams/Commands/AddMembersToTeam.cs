using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Teams.Commands;

public sealed class AddMembersToTeam
{
    public sealed class Command : IRequest<Result<string>>
    {
        public required string TeamId { get; init; }
        public required string[] MemberIds { get; init; } = Array.Empty<string>();
    }

    public sealed class Handler(
        ITeamsRepository teamsRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<string>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            if (string.IsNullOrWhiteSpace(request.TeamId))
                return Result<string>.Failure("TeamId is required.", HttpStatusCode.BadRequest);

            if (request.MemberIds is null || request.MemberIds.Length == 0)
                return Result<string>.Failure("MembersIds is required.", HttpStatusCode.BadRequest);

            try
            {
                await teamsRepository.AddMembersAsync(
                    tenantId: tenantId,
                    teamId: request.TeamId,
                    memberIds: request.MemberIds,
                    cancellationToken: cancellationToken);

                await unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<string>.Success("Members added successfully.", HttpStatusCode.OK);
            }
            catch (InvalidOperationException ex)
            {
                return Result<string>.Failure(ex.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}