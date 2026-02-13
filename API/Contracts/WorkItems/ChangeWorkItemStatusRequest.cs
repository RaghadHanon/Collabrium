namespace API.Contracts.WorkItems;

public class ChangeWorkItemStatusRequest
{
    public required string NewStatus { get; init; }
}