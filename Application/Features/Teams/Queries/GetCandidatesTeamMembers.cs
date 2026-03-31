using Application.Common;
using Application.Features.Users.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using MediatR;
using System.Net;

namespace Application.Features.Teams.Queries;
public class GetCandidatesTeamMembers
{
    public class  Query : IRequest<Result<List<UserDto>>>
    {
        public required string TeamId { get; set; }
    }

    public class Handler(ITeamsReadModel teamsReadModel, ITenantContext tenantContext) : IRequestHandler<Query, Result<List<UserDto>>>
    {
        public async Task<Result<List<UserDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<List<UserDto>>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var team = await teamsReadModel.GetByIdAsync(tenantId, request.TeamId, cancellationToken);
            if (team is null)
                return Result<List<UserDto>>.Failure("Team not found.", HttpStatusCode.NotFound);

            var candidates = await teamsReadModel.GetCandidatesMembers(tenantId, request.TeamId, cancellationToken);

            return Result<List<UserDto>>.Success(candidates, HttpStatusCode.OK);
        }
    }
}
