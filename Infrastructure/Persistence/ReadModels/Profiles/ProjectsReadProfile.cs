using Application.Features.Projects.DTOs;
using Application.Features.WorkItems.DTOs;
using AutoMapper;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.ReadModels.Profiles;

public sealed class ProjectsReadProfile : Profile
{
    public ProjectsReadProfile()
    {
        CreateMap<ProjectEntity, ProjectDto>()
            .ForMember(d => d.TeamName,
                o => o.MapFrom(s => s.Team != null ? s.Team.Name : ""));
    }
}
