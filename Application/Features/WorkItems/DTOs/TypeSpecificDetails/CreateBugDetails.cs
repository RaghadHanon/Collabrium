using Application.Interfaces;

namespace Application.Features.WorkItems.DTOs.TypeSpecificDetails;

public class CreateBugDetails : IWorkItemCreateDetails
{
    public required string StepsToReproduce { get; init; }
    public required string ActualResult { get; init; }
    public required string ExpectedResult { get; init; }
}
