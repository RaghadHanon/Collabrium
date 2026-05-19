using Application.Features.AI.DTOs.WorkItems;
using Domain.Enums;

namespace Application.Interfaces.AI;

public interface IWorkItemFieldsSuggestionService
{
    Task<TSuggestion> GenerateAsync<TSuggestion>(
        WorkItemType workItemType,
        string summary,
        CancellationToken cancellationToken = default)
        where TSuggestion : WorkItemFieldsSuggestionDto;
}