using Application.Features.ProjectsHistories.DTOs;
using AutoMapper;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.ReadModels.Profiles;

public class ProjectHistoriesReadProfile : Profile
{
    public ProjectHistoriesReadProfile()
    {
        CreateMap<ProjectHistoryEntity, ProjectHistoryDto>()
            .ForMember(d => d.MadeByUserName,
                o => o.MapFrom(s => s.MadeByUser.FullName));
    }
}