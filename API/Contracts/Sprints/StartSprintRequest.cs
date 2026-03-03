namespace API.Contracts.Sprints;

public sealed class StartSprintRequest
{
    public string? Name { get; set; }
    public string? Goal { get; set; }
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
}