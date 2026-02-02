using API.Contracts.Teams;
using Application.Features.Teams.Commands;
using Application.Features.Teams.Queries;
using AutoMapper;

namespace API.MappingProfiles;

public class TeamsMappingProfile : Profile
{
    public TeamsMappingProfile()
    {
        CreateMap<CreateTeamRequest, CreateTeam.Command>();
        CreateMap<AddMembersToTeamRequest, AddMembersToTeam.Command>();
        CreateMap<RemoveMembersFromTeamRequest, AddMembersToTeam.Command>();
        CreateMap<GetTeamsRequest, GetTeams.Query>();
        CreateMap<UpdateTeamRequest, UpdateTeam.Command>();
    }
}
