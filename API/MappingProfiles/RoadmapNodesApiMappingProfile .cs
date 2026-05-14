using API.Contracts.RoadmapNodes;
using Application.Features.RoadmapNodes.Commands;
using AutoMapper;

namespace API.MappingProfiles;

public sealed class RoadmapNodesApiMappingProfile : Profile
{
    public RoadmapNodesApiMappingProfile()
    {
        CreateMap<CreateTopicNodeRequest, CreateTopicNode.Command>();
        CreateMap<CreateReferenceNodeRequest, CreateReferenceNode.Command>();

        CreateMap<UpdateTopicNodeRequest, UpdateTopicNode.Command>();
        CreateMap<UpdateReferenceNodeRequest, UpdateReferenceNode.Command>();
    }
}
