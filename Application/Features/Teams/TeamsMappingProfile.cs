using Application.Features.Teams.Commands;
using Application.Features.Teams.DTOs;
using Application.Features.Teams.Queries;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Teams;
public class TeamsMappingProfile : Profile
{
    public TeamsMappingProfile()
    {
        CreateMap<CreateTeam.Command, Team>();
        CreateMap<UpdateTeam.Command, Team>()
            .ForAllMembers(opt =>
                opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Team, TeamDto>()
            .ForMember(d => d.TeamLeadName, opt => opt.MapFrom(s => s.TeamLead.FullName))
            .ForMember(d => d.Count, opt => opt.MapFrom(s => s.Members.Count()));

        CreateMap<Team, TeamFullInfoDto>()
            .ForMember(d => d.TeamLeadName, opt => opt.MapFrom(s => s.TeamLead.FullName))
            .ForMember(d => d.Count, opt => opt.MapFrom(s => s.Members.Count()));

        CreateMap<GetTeams.Query, TeamsSearchAndFilterParametersDto>();
    }
}
