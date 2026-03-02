namespace API.Contracts.Sprints;

public sealed class UpdateSprintRequest
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
