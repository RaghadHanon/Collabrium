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
        if (string.IsNullOrWhiteSpace(projectId))
        {
            return Result<TSuggestion>.Failure(
                "ProjectId is required.",
                HttpStatusCode.BadRequest);
        }

        if (string.IsNullOrWhiteSpace(summary))
        {
            return Result<TSuggestion>.Failure(
                "Summary is required.",
                HttpStatusCode.BadRequest);
        }

        try
        {
            var suggestion = await suggestionService.GenerateAsync<TSuggestion>(
                workItemType,
                summary,
                cancellationToken);

            return Result<TSuggestion>.Success(
                suggestion,
                HttpStatusCode.OK);
        }
        catch (ArgumentException ex)
        {
            return Result<TSuggestion>.Failure(
                ex.Message,
                HttpStatusCode.BadRequest);
        }
        catch (InvalidOperationException ex)
        {
            return Result<TSuggestion>.Failure(
                ex.Message,
                HttpStatusCode.BadRequest);
        }
        catch (NotSupportedException ex)
        {
            return Result<TSuggestion>.Failure(
                ex.Message,
                HttpStatusCode.BadRequest);
        }
    }
}