using Application.Common;
using Application.Events;
using Application.Features.Users.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.Teams.Commands;

public class CreateTeam
{
    public class Command : IRequest<Result<string>>
    {
        public required string TeamLeadId { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public Department Department { get; set; }
        public int Capacity { get; set; }
    }

    public class Handler(
        ITeamsRepository teamRepository,
        ITenantContext tenantContext,
        IUsersRepository usersRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserAccessor currentUserAccessor,
        IMediator mediator
    ) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenant = tenantContext.CurrentTenant;
            if (tenant is null)
                return Result<string>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var currentUser = await currentUserAccessor.GetCurrentUserAsync();
            if (currentUser is null)
                return Result<string>.Failure("Current user is not found.", HttpStatusCode.NotFound);

            var leader = await usersRepository.GetByIdAsync(request.TeamLeadId, cancellationToken);
            if (leader is null || leader.TenantId != tenant.Id)
                return Result<string>.Failure("Team lead not found in the tenant.", HttpStatusCode.NotFound);

            if (await currentUserAccessor.GetUserRole(leader.Id, cancellationToken) != UserRole.TeamLead)
                return Result<string>.Failure("Only team leaders can be assigned to lead the team.", HttpStatusCode.BadRequest);

            var isNameUnique = await teamRepository.IsNameUniqueAsync(
                tenant.Id,
                request.Name,
                cancellationToken);

            if (!isNameUnique)
                return Result<string>.Failure(
                    "A team with this name already exists in the tenant.",
                    HttpStatusCode.BadRequest);

            var team = new Team(
                tenant.Id,
                request.TeamLeadId,
                request.Name,
                request.Department,
                request.Description,
                request.Capacity,
                true);

            await teamRepository.AddAsync(team, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var teamLeadSnapshot = new UserSnapshotDto
            {
                Id = leader.Id,
                FullName = leader.FullName,
                Email = leader.Email,
                JobTitle = leader.JobTitle
            };

            await mediator.Publish(new AddMemberToTeamEvent(
                    tenant.Id,
                    team.Id,
                    new List<UserSnapshotDto> { teamLeadSnapshot },
                    currentUser.Id,
                    currentUser.FullName,
                    leader.FullName,
                    team.Name
                ), cancellationToken);

            return Result<string>.Success(team.Id, HttpStatusCode.Created);
        }
    }
}