using Domain.Enums;

namespace API.Contracts.Sprints;

public sealed class GetSprintsRequest
{
    public string? Code { get; init; }
    public string? NameContains { get; init; }
    public SprintStatus? Status { get; init; }

    public DateTime? StartAfter { get; init; }
    public DateTime? StartBefore { get; init; }

    // Planned end date filters
    public DateTime? EndAfter { get; init; }
    public DateTime? EndBefore { get; init; }

    // Actual start date filters
    public DateTime? StartedAfter { get; init; }
    public DateTime? StartedBefore { get; init; }

    // Actual completion date filters
    public DateTime? CompletedAfter { get; init; }
    public DateTime? CompletedBefore { get; init; }

    public DateTime? CreatedAfter { get; init; }
    public DateTime? CreatedBefore { get; init; }

    public DateTime? Cursor { get; init; }
    public int PageSize { get; init; } = 10;
}
