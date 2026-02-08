using Application.Interfaces;
using Domain.Enums;

namespace Application.Features.WorkItems.DTOs.TypeSpecificDetails;

public class UpdateTestCaseDetails : IWorkItemUpdateDetails
{
    public string? Preconditions { get; set; }
    public string? Steps { get; set; }
    public TestEnvironment? TestEnvironment { get; set; }
    public string? ExpectedResult { get; set; }
    public string? ActualResult { get; set; }
    public TestCaseResult? Result { get; set; }
}