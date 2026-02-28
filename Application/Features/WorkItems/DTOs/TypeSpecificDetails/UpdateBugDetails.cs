using Application.Interfaces;

namespace Application.Features.WorkItems.DTOs.TypeSpecificDetails;

public class UpdateBugDetails : IWorkItemUpdateDetails
{
    public string? StepsToReproduce { get; set; }
    public string? ExpectedResult { get; set; }
    public string? ActualResult { get; set; }
}
