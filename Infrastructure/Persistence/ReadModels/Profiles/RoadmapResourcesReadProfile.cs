using Application.Features.RoadmapResources.DTOs;
using AutoMapper;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.ReadModels.Profiles;

public sealed class RoadmapResourcesReadProfile : Profile
{
    public RoadmapResourcesReadProfile()
    {
        CreateMap<ResourceEntity, ResourceDto>();
    }
}
