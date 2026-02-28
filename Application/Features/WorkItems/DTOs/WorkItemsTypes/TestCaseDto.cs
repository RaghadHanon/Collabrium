using Domain.Enums;

namespace Application.Features.WorkItems.DTOs.WorkItemsTypes;

public class TestCaseDto : WorkItemDto
{
    public string? Preconditions { get; set; }
    public string? Steps { get; set; }
    public TestEnvironment? TestEnvironment { get; set; }
    public TestCaseResult? Result { get; set; }
}