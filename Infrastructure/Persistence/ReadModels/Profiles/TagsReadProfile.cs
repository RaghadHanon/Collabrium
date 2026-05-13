using Application.Features.Tags.DTOs;
using AutoMapper;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.ReadModels.Profiles;

public sealed class TagsReadProfile : Profile
{
    public TagsReadProfile()
    {
        CreateMap<TagEntity, TagDto>();

        CreateMap<TagEntity, TagDetailsDto>();
    }
}
