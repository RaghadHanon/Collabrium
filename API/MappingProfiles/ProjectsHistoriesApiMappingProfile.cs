using API.Contracts.ProjectHistory;
using Application.Features.ProjectsHistories.Queries;
using AutoMapper;

namespace API.MappingProfiles;

public class ProjectsHistoriesApiMappingProfile : Profile
{
    public ProjectsHistoriesApiMappingProfile()
    {
        CreateMap<GetProjectHistoryRequest, GetProjectHistory.Query>();
    }
}