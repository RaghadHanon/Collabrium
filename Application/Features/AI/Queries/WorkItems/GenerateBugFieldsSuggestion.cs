using Application.Common;
using Application.Features.AI.DTOs.WorkItems;
using Application.Interfaces.AI;
using Domain.Enums;
using MediatR;

namespace Application.Features.AI.Queries.WorkItems;

public static class GenerateBugFieldsSuggestion
{
    public class Query : IRequest<Result<BugFieldsSuggestionDto>>
    {
        public string ProjectId { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
    }

    public class Handler(IWorkItemFieldsSuggestionService suggestionService)
        : IRequestHandler<Query, Result<BugFieldsSuggestionDto>>
    {
        public Task<Result<BugFieldsSuggestionDto>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            return WorkItemAiSuggestionHandler.GenerateAsync<BugFieldsSuggestionDto>(
                request.ProjectId,
                request.Summary,
                WorkItemType.Bug,
                suggestionService,
                cancellationToken);
        }
    }
}