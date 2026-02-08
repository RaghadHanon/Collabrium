using Application.Features.Teams.DTOs;
using AutoMapper;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.ReadModels.Profiles;

public sealed class TeamsReadProfile : Profile
{
    public TeamsReadProfile()
    {
        CreateMap<TeamEntity, TeamDto>()
            .ForMember(d => d.TeamLeadName,
                o => o.MapFrom(s => s.TeamLead != null ? s.TeamLead.FullName : ""))
            .ForMember(d => d.Count,
                o => o.MapFrom(s => s.Members.Count));

        CreateMap<TeamEntity, TeamFullInfoDto>()
            .IncludeBase<TeamEntity, TeamDto>()
            .ForMember(d => d.Members, o => o.MapFrom(s => s.Members.Select(m => m.User)));
    }
}
