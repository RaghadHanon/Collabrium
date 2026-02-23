using Domain.Enums;

namespace Infrastructure.Persistence.Entities;

public class TestCaseEntity : WorkItemEntity
{
    public string Preconditions { get; set; } = null!;
    public string Steps { get; set; } = null!;
    public TestEnvironment TestEnvironment { get; set; }
    public string ExpectedResult { get; set; } = null!;
    public string? ActualResult { get; set; }
    public TestCaseResult Result { get; set; } = TestCaseResult.NotRun;
}