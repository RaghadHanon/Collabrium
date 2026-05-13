using API.Contracts.SubTopics;
using Application.Features.SubTopics.Commands;
using AutoMapper;

namespace API.MappingProfiles;

public sealed class SubTopicsProfile : Profile
{
    public SubTopicsProfile()
    {
        CreateMap<CreateSubTopicRequest, CreateSubTopic.Command>();
        CreateMap<UpdateSubTopicRequest, UpdateSubTopic.Command>();
    }
}
