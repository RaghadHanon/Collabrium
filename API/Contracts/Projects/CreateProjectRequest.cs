namespace API.Contracts.Projects;

public sealed class CreateProjectRequest
{
    public required string Name { get; init; }
    public required string Key { get; set; }
    public string? Description { get; init; }
    public string? TeamId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}