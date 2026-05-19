namespace API.Contracts.AI.WorkItems;

public sealed class GenerateWorkItemFieldsSuggestionRequest
{
    public required string Summary { get; init; }
}