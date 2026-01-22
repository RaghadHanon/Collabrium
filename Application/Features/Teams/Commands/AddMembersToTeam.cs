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
        IUsersRepository usersRepository,
        ITenantContext tenantContext
    ) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenant = tenantContext.CurrentTenant;
            if (tenant is null)
            {
                return Result<string>.Failure("Tenant not found.", HttpStatusCode.NotFound);
            }

            var memberIds = request.MemberIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            var team = await teamRepository.GetByIdAsync(tenant.Id, request.TeamId, cancellationToken);
            if (team is null || team.TenantId != tenant.Id)
            {
                return Result<string>.Failure("Team not found in the tenant.", HttpStatusCode.NotFound);
            }

            if (!team.IsActive)
            {
                return Result<string>.Failure("Cannot add members to an inactive team.", HttpStatusCode.BadRequest);
            }

            var users = await usersRepository.GetByIdsAsync(memberIds, cancellationToken);
            var foundIds = users.Select(u => u.Id).ToHashSet(StringComparer.Ordinal);
            var missingIds = memberIds.Where(id => !foundIds.Contains(id)).ToArray();
            if (missingIds.Length > 0)
            {
                return Result<string>.Failure("One or more users were not found.", HttpStatusCode.NotFound);
            }

            var foreignTenantUsers = users.Where(u => u.TenantId != tenant.Id).ToList();
            if (foreignTenantUsers.Count > 0)
            {
                return Result<string>.Failure("One or more users are not in the current tenant.", HttpStatusCode.BadRequest);
            }

            var existingMemberIds = team.Members.Select(m => m.Id).ToHashSet(StringComparer.Ordinal);
            var alreadyInTeam = memberIds.Where(id => existingMemberIds.Contains(id)).ToArray();

            if (alreadyInTeam.Length > 0)
            {
                return Result<string>.Failure("One or more users are already members of the team.", HttpStatusCode.BadRequest);
            }

            var capacity = team.Capacity;
            if (capacity.HasValue)
            {
                var newCount = team.Members.Count + users.Count;
                if (newCount > capacity.Value)
                {
                    return Result<string>.Failure("Adding these members exceeds the team capacity.", HttpStatusCode.BadRequest);
                }
            }

            foreach (var user in users)
            {
                team.Members.Add(user);
            }

            team.UpdatedAt = DateTime.UtcNow;
            await teamRepository.SaveChangesAsync(cancellationToken);

            return Result<string>.Success("Members added successfully.", HttpStatusCode.OK);
        }
    }
}
