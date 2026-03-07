using Application.Features.WorkItems.DTOs;
using Application.Features.WorkItems.DTOs.Attachment;
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

        // common
        CreateMap<ApplicationUserEntity, RelatedUserDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.FullName))
            .ForMember(d => d.Image, opt => opt.MapFrom(s =>
                        s.ProfileImage == null ?
                        null : s.ProfileImage.Url));

        // relations
        CreateMap<WorkItemRelationEntity, WorkItemRelationDto>();

        // comments
        CreateMap<CommentEntity, CommentDto>()
            .ForMember(d => d.RelatedUser, opt => opt.MapFrom(s => s.CreatedByUser));

        // code links
        CreateMap<CodeLinkEntity, CodeLinkDto>()
            .ForMember(d => d.RelatedUser, opt => opt.MapFrom(s => s.CreatedByUser));

        // attachments
        CreateMap<AttachmentEntity, AttachmentDto>()
            .ForMember(d => d.RelatedUser, opt => opt.MapFrom(s => s.UploadedByUser));
    }
}