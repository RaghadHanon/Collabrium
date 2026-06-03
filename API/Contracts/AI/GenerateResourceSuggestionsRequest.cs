namespace API.Contracts.AI;

public sealed class GenerateResourceSuggestionsRequest
{
    public int SuggestionsCount { get; set; } = 3;
}