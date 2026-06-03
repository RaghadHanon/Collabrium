using API.Contracts.AI;
using Application.Features.AI.Queries.Roadmaps;
using AutoMapper;

namespace API.MappingProfiles;

public sealed class AIRoadmapsProfile : Profile
{
    public AIRoadmapsProfile()
    {
        CreateMap<GenerateRoadmapFullDraftRequest, GenerateRoadmapFullDraft.Query>();
        CreateMap<GenerateTopicSuggestionsRequest, GenerateTopicSuggestions.Query>();
        CreateMap<GenerateSubTopicSuggestionsRequest, GenerateSubTopicSuggestions.Query>();
        CreateMap<GenerateResourceSuggestionsRequest, GenerateResourceSuggestions.Query>();
    }
}