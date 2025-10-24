namespace Domain.Entities;

public class TestCase : WorkItem 
{
    public string Preconditions { get; set; } = null!;
    public string Steps { get; set; } = null!;
    public string ExpectedResult { get; set; } = null!;
}
