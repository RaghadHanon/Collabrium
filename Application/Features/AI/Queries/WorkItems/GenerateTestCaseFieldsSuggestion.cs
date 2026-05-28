using Application.Common;
using Application.Features.AI.DTOs.WorkItems;
using Application.Interfaces.AI;
using Domain.Enums;
using MediatR;

namespace Application.Features.AI.Queries.WorkItems;

public static class GenerateTestCaseFieldsSuggestion
{
    public class Query : IRequest<Result<TestCaseFieldsSuggestionDto>>
    {
        public string ProjectId { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
    }

    public class Handler(IWorkItemFieldsSuggestionService suggestionService)
        : IRequestHandler<Query, Result<TestCaseFieldsSuggestionDto>>
    {
        public Task<Result<TestCaseFieldsSuggestionDto>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            return WorkItemAiSuggestionHandler.GenerateAsync<TestCaseFieldsSuggestionDto>(
                request.ProjectId,
                request.Summary,
                WorkItemType.TestCase,
                suggestionService,
                cancellationToken);
        }
    }
}