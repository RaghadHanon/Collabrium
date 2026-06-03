namespace API.Contracts.AI;

public sealed class GenerateTopicSuggestionsRequest
{
    public int SuggestionsCount { get; set; } = 3;
}