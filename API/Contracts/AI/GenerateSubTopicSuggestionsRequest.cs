namespace API.Contracts.AI;

public sealed class GenerateSubTopicSuggestionsRequest
{
    public int SuggestionsCount { get; set; } = 3;
}