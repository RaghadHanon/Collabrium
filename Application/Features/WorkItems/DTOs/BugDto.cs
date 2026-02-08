namespace Application.Features.WorkItems.DTOs;

public class BugDto : WorkItemDto
{
    public string? StepsToReproduce { get; set; }
    public string? ExpectedResult { get; set; }
    public string? ActualResult { get; set; }
}