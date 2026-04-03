using API.Contracts.Teams;
using Application.Features.Teams.Commands;
using Application.Features.Teams.Queries;
using AutoMapper;

namespace API.MappingProfiles;

public class TeamsApiMappingProfile : Profile
{
    public TeamsApiMappingProfile()
    {
        CreateMap<CreateTeamRequest, CreateTeam.Command>();
        CreateMap<AddMembersToTeamRequest, AddMembersToTeam.Command>();
        CreateMap<RemoveMembersFromTeamRequest, RemoveMembersFromTeam.Command>();
        CreateMap<GetTeamsRequest, GetTeams.Query>();
        CreateMap<GetTeamsRequest, GetUserTeams.Query>();
        CreateMap<UpdateTeamRequest, UpdateTeam.Command>();
    }
}
