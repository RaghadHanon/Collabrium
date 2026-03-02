namespace API.Contracts.Sprints;

public sealed class GetSprintsRequest
{
    public string? Name { get; init; }
    public string? Status { get; init; }

    public DateTime? StartAfter { get; init; }
    public DateTime? StartBefore { get; init; }

    public DateTime? EndAfter { get; init; }
    public DateTime? EndBefore { get; init; }

    public DateTime? CreatedAfter { get; init; }
    public DateTime? CreatedBefore { get; init; }

    public DateTime? Cursor { get; init; }
    public int PageSize { get; init; } = 10;
}
