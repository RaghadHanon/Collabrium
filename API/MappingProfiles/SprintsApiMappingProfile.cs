using API.Contracts.Sprints;
using Application.Features.Sprints.Commands;
using Application.Features.Sprints.Queries;
using AutoMapper;

namespace API.MappingProfiles;

public class SprintsApiMappingProfile : Profile
{
    public SprintsApiMappingProfile()
    {
        CreateMap<StartSprintRequest, StartSprint.Command>();
        CreateMap<UpdateSprintRequest, UpdateSprint.Command>();
        CreateMap<GetSprintsRequest, GetSprints.Query>();
        CreateMap<AddWorkItemsToSprintRequest, AddWorkItemsToSprint.Command>();
    }
}
