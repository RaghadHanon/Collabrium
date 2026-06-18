using API.Contracts.AI;
using Application.Features.AI.Queries.Roadmaps;
using Application.Features.Roadmaps.Commands;
using AutoMapper;

namespace API.MappingProfiles;

public sealed class AIRoadmapsProfile : Profile
{
    public AIRoadmapsProfile()
    {
        CreateMap<CreateRoadmapWithAiRequest, CreateRoadmapWithAi.Command>();
        CreateMap<GenerateRoadmapFullDraftRequest, GenerateRoadmapFullDraft.Query>();
        CreateMap<GenerateTopicSuggestionsRequest, GenerateTopicSuggestions.Query>();
        CreateMap<GenerateSubTopicSuggestionsRequest, GenerateSubTopicSuggestions.Query>();
        CreateMap<GenerateResourceSuggestionsRequest, GenerateResourceSuggestions.Query>();
    }
}