using Application.Features.Projects.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Projects;
public class ProjectsMappingProfile : Profile    
{
    public ProjectsMappingProfile()
    {
        CreateMap<Project, ProjectDto>();
    }
}
