using Application.Features.Teams.Commands;
using Application.Features.Teams.DTOs;
using Application.Features.Teams.Queries;
using AutoMapper;
using Domain.Models;

namespace Application.Features.Teams;
public class TeamsMappingProfile : Profile
{
    public TeamsMappingProfile()
    {
        CreateMap<CreateTeam.Command, Team>();
        CreateMap<UpdateTeam.Command, Team>()
            .ForAllMembers(opt =>
                opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Team, TeamDto>();

        CreateMap<Team, TeamFullInfoDto>();

        CreateMap<GetTeams.Query, TeamsSearchAndFilterParametersDto>();
    }
}
