namespace API.Contracts.Sprints;

public sealed class StartSprintRequest
{
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
}