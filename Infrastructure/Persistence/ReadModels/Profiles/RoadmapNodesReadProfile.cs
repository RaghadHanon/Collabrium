using Application.Features.RoadmapNodes.DTOs;
using Application.Features.Roadmaps.DTOs;
using AutoMapper;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.ReadModels.Profiles
{
    public class RoadmapNodesReadProfile : Profile
    {
        public RoadmapNodesReadProfile()
        {
            CreateMap<RoadmapNodeEntity, RoadmapNodeDto>()
                .Include<TopicNodeEntity, TopicNodeDto>()
                .Include<ReferenceNodeEntity, ReferenceNodeDto>();

            CreateMap<TopicNodeEntity, TopicNodeDto>()
                .ForMember(d => d.NodeType, o => o.MapFrom(_ => "Topic"))
                .ForMember(d => d.SubTopicsCount, o => o.MapFrom(s => s.SubTopics.Count));

            CreateMap<ReferenceNodeEntity, ReferenceNodeDto>()
                .ForMember(d => d.NodeType, o => o.MapFrom(_ => "Reference"))
                .ForMember(d => d.RefRoadmapId, o => o.MapFrom(s => s.RefRoadmapId))
                .ForMember(d => d.RefRoadmapTitle,
                    o => o.MapFrom(s => s.RefRoadmap != null ? s.RefRoadmap.Title : null))
                .ForMember(d => d.RefRoadmapIsVisible,
                    o => o.MapFrom(s => s.RefRoadmap != null ? s.RefRoadmap.IsVisible : (bool?)null));

            CreateMap<TopicNodeEntity, TopicNodeDetailsDto>()
                .ForMember(d => d.SubTopicsCount, o => o.MapFrom(s => s.SubTopics.Count))
                .ForMember(d => d.SubTopics, o => o.MapFrom(s => s.SubTopics));

        CreateMap<SubTopicEntity, SubTopicDto>();
    }
}
