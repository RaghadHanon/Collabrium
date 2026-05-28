using Application.Common;
using Application.Features.AI.DTOs.WorkItems;
using Application.Interfaces.AI;
using Domain.Enums;
using MediatR;

namespace Application.Features.AI.Queries.WorkItems;

public static class GenerateUserStoryFieldsSuggestion
{
    public class Query : IRequest<Result<UserStoryFieldsSuggestionDto>>
    {
        public string ProjectId { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
    }

    public class Handler(IWorkItemFieldsSuggestionService suggestionService)
        : IRequestHandler<Query, Result<UserStoryFieldsSuggestionDto>>
    {
        public Task<Result<UserStoryFieldsSuggestionDto>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            return WorkItemAiSuggestionHandler.GenerateAsync<UserStoryFieldsSuggestionDto>(
                request.ProjectId,
                request.Summary,
                WorkItemType.UserStory,
                suggestionService,
                cancellationToken);
        }
    }
}