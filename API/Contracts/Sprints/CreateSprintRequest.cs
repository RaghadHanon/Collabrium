namespace API.Contracts.Sprints;

public sealed class CreateSprintRequest
{
    public required string Name { get; init; }
    public required string Goal { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
}