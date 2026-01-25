using Domain.Enums;

namespace Domain.Models;

public sealed class Bug : WorkItem
{
    public string StepsToReproduce { get; private set; } = null!;
    public string? ActualResult { get; private set; }
    public string? ExpectedResult { get; private set; }

    public Bug(string id, string title, string createdByUserId, string projectId, string stepsToReproduce)
        : base(id, title, createdByUserId, projectId)
    {
        Type = WorkItemType.Bug;
        StepsToReproduce = stepsToReproduce;
    }

    public void UpdateRepro(string steps, string? expected, string? actual)
    {
        StepsToReproduce = steps;
        ExpectedResult = expected;
        ActualResult = actual;
    }
}
