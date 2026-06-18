namespace API.Contracts.Sprints;

public sealed class StartSprintRequest
{
    public string? Name { get; set; }
    public string? Goal { get; set; }
}