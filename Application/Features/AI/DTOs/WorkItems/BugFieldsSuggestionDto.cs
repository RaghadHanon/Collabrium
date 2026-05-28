namespace Application.Features.AI.DTOs.WorkItems;

public sealed class BugFieldsSuggestionDto : WorkItemFieldsSuggestionDto
{
    public string StepsToReproduce { get; set; } = string.Empty;
    public string ExpectedResult { get; set; } = string.Empty;
    public string ActualResult { get; set; } = string.Empty;
}
