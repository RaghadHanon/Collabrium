using Application.Features.Teams.DTOs;
using Application.Features.Teams.Queries;
using AutoMapper;
using Domain.Models;

namespace Application.Features.Teams;
public class TeamsMappingProfile : Profile
{
    public TeamsMappingProfile()
    {
        CreateMap<Team, TeamDto>();
        CreateMap<Team, TeamFullInfoDto>();
        CreateMap<GetTeams.Query, TeamsSearchAndFilterParametersDto>();
    }
}
