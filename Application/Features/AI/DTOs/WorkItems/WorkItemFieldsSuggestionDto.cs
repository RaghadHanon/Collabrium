using Domain.Enums;

namespace Application.Features.AI.DTOs.WorkItems;

public class WorkItemFieldsSuggestionDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AcceptanceCriteria { get; set; }
    public int EffortPoints { get; set; }
    public WorkItemPriority Priority { get; set; }
}