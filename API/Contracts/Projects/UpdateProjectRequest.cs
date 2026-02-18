namespace API.Contracts.Projects;

public sealed class UpdateProjectRequest
{
    public string? Name { get; set; }
    public string? Description { get; init; }
    public string? Status { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}