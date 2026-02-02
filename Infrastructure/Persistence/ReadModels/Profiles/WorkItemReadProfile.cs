using Application.Features.WorkItems.DTOs;
using AutoMapper;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.ReadModels.Profiles;

public class WorkItemReadProfile : Profile
{
    public WorkItemReadProfile()
    {
        CreateMap<WorkItemEntity, WorkItemDto>();
    }
}
