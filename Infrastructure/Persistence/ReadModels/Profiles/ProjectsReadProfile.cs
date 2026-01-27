using Application.Features.Projects.DTOs;
using AutoMapper;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.ReadModels.Profiles;
public class ProjectsReadProfile : Profile
{
    public ProjectsReadProfile()
    {
        CreateMap<ProjectEntity, ProjectDto>();
    }
}
