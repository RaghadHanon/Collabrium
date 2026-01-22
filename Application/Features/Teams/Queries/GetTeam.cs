using Application.Common;
using Application.Features.Projects.DTOs;
using Application.Features.Teams.DTOs;
using Application.Features.Users.DTOs;
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

    public class Handler(ITenantContext tenantContext, ITeamsRepository teamRepository,
        IMapper mapper) : IRequestHandler<Query, Result<TeamFullInfoDto>>
    {
        public async Task<Result<TeamFullInfoDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var team = await teamRepository.GetTeamByIdAsync(tenantContext.TenantId!, request.TeamId, cancellationToken);
            if (team is null)
            {
                return Result<TeamFullInfoDto>.Failure("Team not found", HttpStatusCode.NotFound);
            }

            var teamDto = mapper.Map<TeamFullInfoDto>(team);
            var membersDto = team.Members
                .Select(m => mapper.Map<UserDto>(m))
                .ToList();
            var projectsDto = team.Projects
                .Select(p => mapper.Map<ProjectDto>(p))
                .ToList();

            teamDto.Members = membersDto;
            teamDto.Projects = projectsDto;

            return Result<TeamFullInfoDto>.Success(teamDto, HttpStatusCode.OK);
        }
    }
}
