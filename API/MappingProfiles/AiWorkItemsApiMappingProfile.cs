using API.Contracts.AI.WorkItems;
using Application.Features.AI.Queries.WorkItems;
using AutoMapper;

namespace API.MappingProfiles;

public sealed class AiWorkItemsApiMappingProfile : Profile
{
    public AiWorkItemsApiMappingProfile()
    {
        CreateMap<GenerateWorkItemFieldsSuggestionRequest, GenerateEpicFieldsSuggestion.Query>()
            .ForMember(dest => dest.ProjectId, opt => opt.Ignore());

        CreateMap<GenerateWorkItemFieldsSuggestionRequest, GenerateUserStoryFieldsSuggestion.Query>()
            .ForMember(dest => dest.ProjectId, opt => opt.Ignore());

        CreateMap<GenerateWorkItemFieldsSuggestionRequest, GenerateTaskFieldsSuggestion.Query>()
            .ForMember(dest => dest.ProjectId, opt => opt.Ignore());

        CreateMap<GenerateWorkItemFieldsSuggestionRequest, GenerateBugFieldsSuggestion.Query>()
            .ForMember(dest => dest.ProjectId, opt => opt.Ignore());

        CreateMap<GenerateWorkItemFieldsSuggestionRequest, GenerateTestCaseFieldsSuggestion.Query>()
            .ForMember(dest => dest.ProjectId, opt => opt.Ignore());
    }
}