namespace Application.Features.AI.DTOs.WorkItems;

public sealed class UserStoryFieldsSuggestionDto : WorkItemFieldsSuggestionDto
{
    public int StoryPoints { get; set; }
}
