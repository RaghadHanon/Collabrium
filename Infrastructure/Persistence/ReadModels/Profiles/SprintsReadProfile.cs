using Application.Features.Sprints.DTOs;
using AutoMapper;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.ReadModels.Profiles;

public sealed class SprintsReadProfile : Profile
{
    public SprintsReadProfile()
    {
        CreateMap<SprintEntity, SprintDto>()
            .ForMember(d => d.WorkItemsCount, o => o.MapFrom(s => s.WorkItems.Count));
    }
}