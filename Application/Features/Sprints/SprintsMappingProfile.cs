using Application.Features.Sprints.DTOs;
using Application.Features.Sprints.Queries;
using AutoMapper;

namespace Application.Features.Sprints;

public class SprintsMappingProfile : Profile
{
    public SprintsMappingProfile()
    {
        CreateMap<GetSprints.Query, SprintsSearchAndFilterParametersDto>();
    }
}
