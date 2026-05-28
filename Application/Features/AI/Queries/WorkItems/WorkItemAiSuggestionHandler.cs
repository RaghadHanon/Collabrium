using Application.Common;
using Application.Features.AI.DTOs.WorkItems;
using Application.Interfaces.AI;
using Domain.Enums;
using System.Net;

namespace Application.Features.AI.Queries.WorkItems;

internal static class WorkItemAiSuggestionHandler
{
    public static async Task<Result<TSuggestion>> GenerateAsync<TSuggestion>(
        string projectId,
        string summary,
        WorkItemType workItemType,
        IWorkItemFieldsSuggestionService suggestionService,
        CancellationToken cancellationToken)
        where TSuggestion : WorkItemFieldsSuggestionDto
    {
        var suggestion = await suggestionService.GenerateAsync<TSuggestion>(
            workItemType,
            summary,
            cancellationToken);

        return Result<TSuggestion>.Success(
            suggestion,
            HttpStatusCode.OK);
    }
}