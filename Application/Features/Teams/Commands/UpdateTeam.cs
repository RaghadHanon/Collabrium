using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using AutoMapper;
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
        ITeamsRepository teamRepository,
        ITenantContext tenantContext,
        IUsersRepository usersRepository
        ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenant = tenantContext.CurrentTenant;
            if (tenant is null)
                return Result<Unit>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var existing = await teamRepository.GetByIdAsync(tenant.Id, request.Id, cancellationToken);
            if (existing is null)
                return Result<Unit>.Failure("Team not found in this tenant.", HttpStatusCode.NotFound);

            if (!existing.IsActive)
                return Result<Unit>.Failure("Cannot update an inactive team.", HttpStatusCode.BadRequest);

            if (request.TeamLeadId != null)
            {
                var leader = await usersRepository.GetByIdAsync(request.TeamLeadId, cancellationToken);
                if (leader is null || leader.TenantId != tenant.Id)
                    return Result<Unit>.Failure("Team lead not found in the tenant.", HttpStatusCode.NotFound);

                existing.ChangeLead(request.TeamLeadId);
            }

            if (request.Name != null)
            {
                var isNameUnique = await teamRepository.IsTeamNameUniqueAsync(tenant.Id, request.Name, cancellationToken);
                if (!isNameUnique)
                    return Result<Unit>.Failure("A team with this name already exists in the tenant.", HttpStatusCode.BadRequest);
            }

            var newName = request.Name ?? existing.Name;
            var newDescription = request.Description ?? existing.Description;
            var newCapacity = request.Capacity ?? existing.Capacity;

            existing.UpdateDetails(newName, newDescription, newCapacity);

            await teamRepository.UpdateTeamAsync(existing, cancellationToken);
            await teamRepository.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}