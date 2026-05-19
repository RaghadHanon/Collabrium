using Application.Common;
using Application.Features.AI.DTOs.WorkItems;
using Application.Interfaces.AI;
using Domain.Enums;
using MediatR;

namespace Application.Features.AI.Queries.WorkItems;

public static class GenerateEpicFieldsSuggestion
{
    public class Query : IRequest<Result<EpicFieldsSuggestionDto>>
    {
        public string ProjectId { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
    }

    public class Handler(IWorkItemFieldsSuggestionService suggestionService)
        : IRequestHandler<Query, Result<EpicFieldsSuggestionDto>>
    {
        public Task<Result<EpicFieldsSuggestionDto>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            return WorkItemAiSuggestionHandler.GenerateAsync<EpicFieldsSuggestionDto>(
                request.ProjectId,
                request.Summary,
                WorkItemType.Epic,
                suggestionService,
                cancellationToken);
        }
    }
}