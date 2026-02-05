namespace API.Contracts.Sprints;

public sealed class UpdateSprintRequest
{
    public string? Name { get; init; }
    public string? Goal { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
