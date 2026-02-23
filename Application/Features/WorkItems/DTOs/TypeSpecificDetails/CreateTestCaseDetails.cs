using Application.Interfaces;
using Domain.Enums;

namespace Application.Features.WorkItems.DTOs.TypeSpecificDetails;

public class CreateTestCaseDetails : IWorkItemCreateDetails
{
    public required string Preconditions { get; init; }
    public required string Steps { get; init; }
    public required TestEnvironment TestEnvironment { get; init; }
    public required string ExpectedResult { get; init; }
}