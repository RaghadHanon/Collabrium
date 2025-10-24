namespace Domain.Entities;

public class Bug : WorkItem
{
    public string StepsToReproduce { get; set; } = null!;
    public string? ActualResult { get; set; }
    public string? ExpectedResult { get; set; }
}
