using Application.Features.WorkItems.DTOs;
using Application.Features.WorkItems.DTOs.CodeLinkes;
using Application.Features.WorkItems.DTOs.Comments;
using Application.Features.WorkItems.DTOs.Relations;
using Application.Features.WorkItems.DTOs.WorkItemsTypes;
using AutoMapper;
using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.ReadModels.Profiles;

public sealed class WorkItemsReadProfile : Profile
{
    public WorkItemsReadProfile()
    {
        CreateMap<WorkItemEntity, WorkItemDto>()
            .Include<EpicEntity, EpicDto>()
            .Include<UserStoryEntity, UserStoryDto>()
            .Include<TaskItemEntity, TaskDto>()
            .Include<BugEntity, BugDto>()
            .Include<TestCaseEntity, TestCaseDto>();

        CreateMap<EpicEntity, EpicDto>()
            .IncludeBase<WorkItemEntity, WorkItemDto>();

        CreateMap<UserStoryEntity, UserStoryDto>()
            .IncludeBase<WorkItemEntity, WorkItemDto>();

        CreateMap<TaskItemEntity, TaskDto>()
            .IncludeBase<WorkItemEntity, WorkItemDto>();

        CreateMap<BugEntity, BugDto>()
            .IncludeBase<WorkItemEntity, WorkItemDto>();

        CreateMap<TestCaseEntity, TestCaseDto>()
            .IncludeBase<WorkItemEntity, WorkItemDto>();

        CreateMap<WorkItemEntity, WorkItemListItemDto>();

        CreateMap<WorkItemEntity, RelationParticipantWorkItemDto>();

        // relations
        CreateMap<WorkItemRelationEntity, WorkItemRelationDto>();

        // comments
        CreateMap<CommentEntity, CommentDto>();

        // code links
        CreateMap<CodeLinkEntity, CodeLinkDto>();
    }
}