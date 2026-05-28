using Application.Common;
using Application.Features.AI.DTOs.WorkItems;
using Application.Interfaces.AI;
using Domain.Enums;
using MediatR;

namespace Application.Features.AI.Queries.WorkItems;

public static class GenerateTaskFieldsSuggestion
{
    public class Query : IRequest<Result<TaskFieldsSuggestionDto>>
    {
        public string ProjectId { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
    }

    public class Handler(IWorkItemFieldsSuggestionService suggestionService)
        : IRequestHandler<Query, Result<TaskFieldsSuggestionDto>>
    {
        public Task<Result<TaskFieldsSuggestionDto>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            return WorkItemAiSuggestionHandler.GenerateAsync<TaskFieldsSuggestionDto>(
                request.ProjectId,
                request.Summary,
                WorkItemType.TaskItem,
                suggestionService,
                cancellationToken);
        }
    }
}