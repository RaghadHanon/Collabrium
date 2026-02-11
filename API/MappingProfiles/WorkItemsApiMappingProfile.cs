using API.Contracts.WorkItems;
using API.Contracts.WorkItems.Create;
using API.Contracts.WorkItems.Update;
using Application.Features.WorkItems.Commands;
using Application.Features.WorkItems.DTOs;
using Application.Features.WorkItems.DTOs.TypeSpecificDetails;
using Application.Interfaces;
using AutoMapper;
using Domain.Enums;

namespace API.MappingProfiles;

public sealed class WorkItemsApiProfile : Profile
{
    public WorkItemsApiProfile()
    {
        CreateMap<GetWorkItemsRequest, WorkItemsSearchAndFilterParametersDto>();
        CreateMap<UpdateWorkItemRequest, UpdateWorkItem.Command>();
        CreateMap<ChangeWorkItemStatusRequest, ChangeWorkItemStatus.Command>();

        CreateMap<CreateEpicRequest, CreateWorkItem.Command>()
            .ForMember(d => d.Type, opt => opt.MapFrom(_ => WorkItemType.Epic))
            .ForMember(d => d.Details, opt => opt.MapFrom(_ => (IWorkItemCreateDetails?) null));

        CreateMap<CreateTaskRequest, CreateWorkItem.Command>()
            .ForMember(d => d.Type, opt => opt.MapFrom(_ => WorkItemType.TaskItem))
            .ForMember(d => d.Details, opt => opt.MapFrom(_ => (IWorkItemCreateDetails?) null));

        CreateMap<CreateUserStoryRequest, CreateWorkItem.Command>()
            .ForMember(d => d.Type, opt => opt.MapFrom(_ => WorkItemType.UserStory))
            .ForMember(d => d.Details, opt => opt.MapFrom(s => new CreateUserStoryDetails
            {
                StoryPoints = s.StoryPoints
            }));

        CreateMap<CreateBugRequest, CreateWorkItem.Command>()
            .ForMember(d => d.Type, opt => opt.MapFrom(_ => WorkItemType.Bug))
            .ForMember(d => d.Details, opt => opt.MapFrom(s => new CreateBugDetails
            {
                StepsToReproduce = s.StepsToReproduce
            }));

        CreateMap<CreateTestCaseRequest, CreateWorkItem.Command>()
            .ForMember(d => d.Type, opt => opt.MapFrom(_ => WorkItemType.TestCase))
            .ForMember(d => d.Details, opt => opt.MapFrom(s => new CreateTestCaseDetails
            {
                Preconditions = s.Preconditions,
                Steps = s.Steps,
                TestEnvironment = Enum.Parse<TestEnvironment>(s.TestEnvironment, true),
                ExpectedResult = s.ExpectedResult
            }));


        CreateMap<UpdateEpicRequest, UpdateWorkItem.Command>()
          .ForMember(d => d.Type, o => o.MapFrom(_ => WorkItemType.Epic))
          .ForMember(d => d.Details, o => o.MapFrom(_ => (IWorkItemUpdateDetails?) null));

        CreateMap<UpdateTaskRequest, UpdateWorkItem.Command>()
            .ForMember(d => d.Type, o => o.MapFrom(_ => WorkItemType.TaskItem))
            .ForMember(d => d.Details, o => o.MapFrom(_ => (IWorkItemUpdateDetails?) null));

        CreateMap<UpdateUserStoryRequest, UpdateWorkItem.Command>()
            .ForMember(d => d.Type, o => o.MapFrom(_ => WorkItemType.UserStory))
            .ForMember(d => d.Details, o => o.MapFrom(s => new UpdateUserStoryDetails
            {
                StoryPoints = s.StoryPoints
            }));

        CreateMap<UpdateBugRequest, UpdateWorkItem.Command>()
            .ForMember(d => d.Type, o => o.MapFrom(_ => WorkItemType.Bug))
            .ForMember(d => d.Details, o => o.MapFrom(s => new UpdateBugDetails
            {
                StepsToReproduce = s.StepsToReproduce,
                ExpectedResult = s.ExpectedResult,
                ActualResult = s.ActualResult
            }));

        CreateMap<UpdateTestCaseRequest, UpdateWorkItem.Command>()
            .ForMember(d => d.Type, o => o.MapFrom(_ => WorkItemType.TestCase))
            .ForMember(d => d.Details, o => o.MapFrom(s => new UpdateTestCaseDetails
            {
                Preconditions = s.Preconditions,
                Steps = s.Steps,
                ExpectedResult = s.ExpectedResult,
                ActualResult = s.ActualResult,
                Result = string.IsNullOrWhiteSpace(s.Result) ? null : Enum.Parse<TestCaseResult>(s.Result, true),
                TestEnvironment = string.IsNullOrWhiteSpace(s.TestEnvironment) ? null : Enum.Parse<TestEnvironment>(s.TestEnvironment, true),
            }));
    }
}