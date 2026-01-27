using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Teams.Commands;

public class AddMembersToTeam
{
    public class Command : IRequest<Result<string>>
    {
        public required string TeamId { get; set; }
        public required string[] MemberIds { get; set; } = Array.Empty<string>();
    }

    public class Handler(
        ITeamsRepository teamRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenant = tenantContext.CurrentTenant;
            if (tenant is null)
                return Result<string>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            try
            {
                await teamRepository.AddMembersAsync(tenant.Id, request.TeamId, request.MemberIds, cancellationToken);
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
