using API.Contracts.Sprints;
using Application.Features.Sprints.Commands;
using Application.Features.Sprints.Queries;
using AutoMapper;

namespace API.MappingProfiles;

public class SprintsApiProfile : Profile
{
    public SprintsApiProfile()
    {
        CreateMap<CreateSprintRequest, CreateSprint.Command>();
        CreateMap<UpdateSprintRequest, UpdateSprint.Command>();
        CreateMap<GetSprintsRequest, GetSprints.Query>();
    }
}
