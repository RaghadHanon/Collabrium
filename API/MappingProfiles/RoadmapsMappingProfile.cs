using API.Contracts.Roadmaps;
using Application.Features.Roadmaps.Commands;
using Application.Features.Roadmaps.Queries;
using AutoMapper;

namespace API.MappingProfiles
{
    public class RoadmapsMappingProfile : Profile
    {
        public RoadmapsMappingProfile()
        {
            CreateMap<CreateRoadmapRequest, CreateRoadmap.Command>();
            CreateMap<AddTagsRequest, AddTags.Command>();
            CreateMap<RemoveTagsRequest, RemoveTags.Command>();
            CreateMap<SetTagsRequest, SetTags.Command>();
            CreateMap<UpdateRoadmapRequest, UpdateRoadmapDetails.Command>();
            CreateMap<ToggleRoadmapVisibleRequest, ToggleRoadmapVisible.Command>();
            CreateMap<GetRoadmapsRequest, GetRoadmaps.Query>();
            CreateMap<GetManagerRoadmapsRequest, GetRoadmaps.Query>();
            CreateMap<GetPublicRoadmapsRequest, GetRoadmaps.Query>();
        }
    }
}
