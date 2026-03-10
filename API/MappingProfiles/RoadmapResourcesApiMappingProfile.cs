using API.Contracts.RoadmapResources;
using Application.Features.RoadmapResources.Commands;
using AutoMapper;

namespace API.MappingProfiles
{
    public class RoadmapResourcesApiMappingProfile : Profile
    {
        public RoadmapResourcesApiMappingProfile()
        {
            CreateMap<CreateResourceRequest, CreateResource.Command>();
            CreateMap<UpdateResourceRequest, UpdateResource.Command>();
        }
    }
}
