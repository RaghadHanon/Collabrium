using API.Contracts.Projects;
using API.Contracts.WorkItems;
using Application.Features.Projects.Commands;
using Application.Features.Projects.Queries;
using AutoMapper;

namespace API.MappingProfiles;

public class ProjectsApiMappingProfile : Profile
{
    public ProjectsApiMappingProfile()
    {
        CreateMap<CreateProjectRequest, CreateProject.Command>();
        CreateMap<UpdateProjectRequest, UpdateProject.Command>();
        CreateMap<AssignProjectToTeamRequest, AssignProjectToTeam.Command>();
        CreateMap<GetProjectsRequest, GetProjects.Query>();
        CreateMap<GetProjectBoardRequest, GetProjectBoard.Query>();
        CreateMap<GetProjectBacklogRequest, GetProjectBacklog.Query>();
    }
}