using Application.Features.Roadmaps.DTOs;
using Application.Features.Tags.DTOs;
using AutoMapper;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.ReadModels.Profiles;

public sealed class RoadmapsReadProfile : Profile
{
    public RoadmapsReadProfile()
    {
        CreateMap<TagEntity, TagDto>();


        CreateMap<RoadmapEntity, RoadmapDto>()
            .ForMember(d => d.CreatedByUserName,
                o => o.MapFrom(s => s.CreatedByUser != null ? s.CreatedByUser.FullName : null))
            .ForMember(d => d.TagsCount,
                o => o.MapFrom(s => s.Tags.Count))
            .ForMember(d => d.NodesCount,
                o => o.MapFrom(s => s.Nodes.Count))
            .ForMember(d => d.Tags,
                o => o.MapFrom(s => s.Tags));
    }
}


