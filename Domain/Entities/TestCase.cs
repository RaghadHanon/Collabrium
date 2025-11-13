using Domain.Enums;

namespace Domain.Entities;

public class TestCase : WorkItem
{
    public string Preconditions { get; set; } = null!;
    public string Steps { get; set; } = null!;
    public TestEnvironment TestEnvironment { get; set; }
    public string ExpectedResult { get; set; } = null!;
    public string ActualResult { get; set; } = null!;
}
