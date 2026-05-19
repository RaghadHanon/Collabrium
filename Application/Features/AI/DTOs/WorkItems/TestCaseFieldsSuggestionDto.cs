using Domain.Enums;

namespace Application.Features.AI.DTOs.WorkItems;

public sealed class TestCaseFieldsSuggestionDto : WorkItemFieldsSuggestionDto
{
    public string Preconditions { get; set; } = string.Empty;
    public string Steps { get; set; } = string.Empty;
    public TestEnvironment TestEnvironment { get; set; }
    public string ExpectedResult { get; set; } = string.Empty;
}