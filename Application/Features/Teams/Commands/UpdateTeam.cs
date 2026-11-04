using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.Teams.Commands;

public class UpdateTeam
{
    public class Command : IRequest<Result<Unit>>
    {
        public required string Id { get; set; }
        public string? TeamLeadId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Department? Department { get; set; }
        public int? Capacity { get; set; }
    }

    public class Handler(
            ITeamsRepository teamsRepository,
            ITenantContext tenantContext,
            IUsersRepository usersRepository,
            ICurrentUserAccessor currentUserAccessor,
            IUnitOfWork unitOfWork) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<Unit>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var team = await teamsRepository.GetByIdWithMembersAsync(tenantId, request.Id, cancellationToken);
            if (team is null)
                return Result<Unit>.Failure("Team not found in this tenant.", HttpStatusCode.NotFound);

            if (!team.IsActive)
                return Result<Unit>.Failure("Cannot update an inactive team.", HttpStatusCode.BadRequest);

            var nameDecision = PatchHelpers.DecideStringPatch(request.Name);
            var descDecision = PatchHelpers.DecideStringPatch(request.Description);
            var leadDecision = PatchHelpers.DecideStringPatch(request.TeamLeadId);

            if (nameDecision.Action == PatchHelpers.PatchStringAction.Set)
            {
                var newName = nameDecision.Value!;
                var isUnique = await teamsRepository.IsNameUniqueAsync(tenantId, newName, cancellationToken);

                if (!isUnique && !string.Equals(team.Name, newName, StringComparison.OrdinalIgnoreCase))
                    return Result<Unit>.Failure("A team with this name already exists in the tenant.", HttpStatusCode.BadRequest);
            }

            if (leadDecision.Action == PatchHelpers.PatchStringAction.Set)
            {
                var leaderId = leadDecision.Value!;
                var leader = await usersRepository.GetByIdAsync(leaderId, cancellationToken);

                if (leader is null || !string.Equals(leader.TenantId, tenantId, StringComparison.Ordinal))
                    return Result<Unit>.Failure("Team lead not found in the tenant.", HttpStatusCode.NotFound);

                if (await currentUserAccessor.GetUserRole(leader.Id, cancellationToken) != UserRole.TeamLead)
                    return Result<Unit>.Failure("Only team leaders can be assigned to lead the team.", HttpStatusCode.BadRequest);
            }

            await unitOfWork.ExecuteInTransactionAsync(async cancellationToken =>
            {
                if (leadDecision.Action == PatchHelpers.PatchStringAction.Clear)
                {
                    team.ChangeLead(null);
                }
                else if (leadDecision.Action == PatchHelpers.PatchStringAction.Set &&
                         !string.Equals(leadDecision.Value, team.TeamLeadId, StringComparison.Ordinal))
                {
                    team.ChangeLead(leadDecision.Value);
                }

                team.ChangeCapacity(request.Capacity ?? team.Capacity);
                team.UpdateDetails(
                    PatchHelpers.ApplyStringPatch(team.Name, request.Name) ?? team.Name,
                    PatchHelpers.ApplyStringPatch(team.Description, request.Description),
                    request.Department ?? team.Department
                );

                await teamsRepository.UpdateAsync(team, cancellationToken);
            }, cancellationToken);

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}