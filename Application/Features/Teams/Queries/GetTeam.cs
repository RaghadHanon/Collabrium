using Application.Common;
using Application.Features.Teams.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;
using System.Net;

namespace Application.Features.Teams.Queries;
public class GetTeam
{
    public class Query : IRequest<Result<TeamFullInfoDto>>
    {
        public required string TeamId { get; set; }
    }

    public class Handler(ITenantContext tenantContext, ITeamsRepository teamRepository)
        : IRequestHandler<Query, Result<TeamFullInfoDto>>
    {
        public async Task<Result<TeamFullInfoDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.TenantId;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<TeamFullInfoDto>.Failure("Tenant not found", HttpStatusCode.NotFound);

            var dto = await teamRepository.GetTeamFullAsync(tenantId, request.TeamId, cancellationToken);
            return dto == null
                ? Result<TeamFullInfoDto>.Failure("Team not found", HttpStatusCode.NotFound)
                : Result<TeamFullInfoDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}
