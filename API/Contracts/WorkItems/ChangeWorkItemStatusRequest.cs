using Domain.Enums;

namespace API.Contracts.WorkItems;

public class ChangeWorkItemStatusRequest
{
    public required WorkItemStatus NewStatus { get; init; }
}