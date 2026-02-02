using API.Contracts.Projects;
using Application.Features.Projects.Commands;
using Application.Features.Projects.Queries;
using AutoMapper;

namespace API.MappingProfiles;

public class ProjectsMappingProfile : Profile
{
    public ProjectsMappingProfile()
    {
        CreateMap<CreateProjectRequest, CreateProject.Command>();
        CreateMap<UpdateProjectRequest, UpdateProject.Command>();
        CreateMap<AssignProjectToTeamRequest, AssignProjectToTeam.Command>();
        CreateMap<GetProjectsRequest, GetProjects.Query>();
    }
}
