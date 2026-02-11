using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore.Infrastructure;
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

            var team = await teamsRepository.GetByIdAsync(tenantId, request.Id, cancellationToken);
            if (team is null)
                return Result<Unit>.Failure("Team not found in this tenant.", HttpStatusCode.NotFound);

            if (!team.IsActive)
                return Result<Unit>.Failure("Cannot update an inactive team.", HttpStatusCode.BadRequest);

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                var isUnique = await teamsRepository.IsNameUniqueAsync(tenantId, request.Name, cancellationToken);
                if (!isUnique && !string.Equals(team.Name, request.Name, StringComparison.OrdinalIgnoreCase))
                    return Result<Unit>.Failure("A team with this name already exists in the tenant.", HttpStatusCode.BadRequest);
            }

            if (!string.IsNullOrWhiteSpace(request.TeamLeadId))
            {
                var leader = await usersRepository.GetByIdAsync(request.TeamLeadId, cancellationToken);
                if (leader is null || !string.Equals(leader.TenantId, tenantId, StringComparison.Ordinal))
                    return Result<Unit>.Failure("Team lead not found in the tenant.", HttpStatusCode.NotFound);

                if(await currentUserAccessor.GetUserRole(leader.Id, cancellationToken) != UserRole.TeamLead.ToString())
                    return Result<Unit>.Failure("Only team leaders can be assigned to lead the team.", HttpStatusCode.BadRequest);
            }

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                if (!string.IsNullOrWhiteSpace(request.TeamLeadId) &&
                    !string.Equals(request.TeamLeadId, team.TeamLeadId, StringComparison.Ordinal))
                {
                    team.ChangeLead(request.TeamLeadId);
                }

                team.UpdateDetails(
                    request.Name ?? team.Name,
                    request.Description ?? team.Description,
                    request.Capacity ?? team.Capacity,
                    request.Department ?? team.Department
                    );

                await teamsRepository.UpdateAsync(team, ct);
            }, cancellationToken);

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}