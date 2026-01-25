namespace Infrastructure.Persistence.Entities;

public class BugEntity : WorkItemEntity
{
    public string StepsToReproduce { get; set; } = null!;
    public string? ActualResult { get; set; }
    public string? ExpectedResult { get; set; }
}
