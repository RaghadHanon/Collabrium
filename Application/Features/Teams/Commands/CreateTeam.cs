using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
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
        IMapper mapper,
        IUsersRepository usersRepository
    ) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenant = tenantContext.CurrentTenant;
            if (tenant is null)
            {
                return Result<string>.Failure("Tenant not found.", HttpStatusCode.NotFound);
            }

            var leader = await usersRepository.GetByIdAsync(request.TeamLeadId);
            if (leader is null || leader.TenantId != tenant.Id)
            {
                return Result<string>.Failure("Team lead not found in the tenant.", HttpStatusCode.NotFound);
            }

            var isNameUnique = await teamRepository
                .IsTeamNameUniqueAsync(tenant.Id, request.Name, cancellationToken);

            if (!isNameUnique)
            {
                return Result<string>.Failure("A team with this name already exists in the tenant.",
                    HttpStatusCode.BadRequest);
            }

            var team = mapper.Map<Team>(request);
            team.TenantId = tenant.Id;
            team.Members.Add(leader);
            team.IsActive = true;
            team.UpdatedAt = DateTime.UtcNow;

            await teamRepository.AddTeamAsync(team, cancellationToken);
            await teamRepository.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(team.Id, HttpStatusCode.Created);
        }
    }
}
