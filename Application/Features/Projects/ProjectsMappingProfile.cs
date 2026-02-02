using Application.Features.Projects.DTOs;
using Application.Features.Projects.Queries;
using AutoMapper;
using Domain.Models;

namespace Application.Features.Projects;
public class ProjectsMappingProfile : Profile
{
    public ProjectsMappingProfile()
    {
        CreateMap<Project, ProjectDto>();
        CreateMap<GetProjects.Query, ProjectsSearchAndFilterParametersDto>();
    }
}
