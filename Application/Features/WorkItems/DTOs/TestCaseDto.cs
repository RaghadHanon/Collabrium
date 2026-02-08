using Domain.Enums;

namespace Application.Features.WorkItems.DTOs;

public class TestCaseDto : WorkItemDto
{
    public string? Preconditions { get; set; }
    public string? Steps { get; set; }
    public TestEnvironment? TestEnvironment { get; set; }
    public TestCaseResult? Result { get; set; }
}