using API.Contracts.Tags;
using Application.Features.Tags.Commands;
using Application.Features.Tags.Queries;
using AutoMapper;

namespace API.MappingProfiles;

public class TagApiMappingProfile : Profile
{
    public TagApiMappingProfile()
    {
        CreateMap<CreateTagRequest, CreateTag.Command>();
        CreateMap<UpdateTagRequest, UpdateTag.Command>();
        CreateMap<GetTagsRequest, GetTags.Query>();


    }
}
