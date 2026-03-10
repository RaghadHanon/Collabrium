using Application.Features.Roadmaps.DTOs;
using AutoMapper;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.ReadModels.Profiles
{
    public sealed class SubTopicsReadProfile : Profile
    {
        public SubTopicsReadProfile()
        {
            CreateMap<SubTopicEntity, SubTopicDto>();

        }
    }
}
