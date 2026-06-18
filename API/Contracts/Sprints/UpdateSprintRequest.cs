namespace API.Contracts.Sprints;

public sealed class UpdateSprintRequest
{
    public string? Name { get; set; }
    public string? Goal { get; set; }
    public DateTime? PlannedStartDate { get; init; }
    public DateTime? PlannedEndDate { get; init; }
}
