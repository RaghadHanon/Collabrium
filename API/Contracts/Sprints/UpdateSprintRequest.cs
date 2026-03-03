namespace API.Contracts.Sprints;

public sealed class UpdateSprintRequest
{
    public string? Name { get; set; }
    public string? Goal { get; set; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
