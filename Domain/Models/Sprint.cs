using Domain.Common;
using Domain.Enums;

namespace Domain.Models;

public sealed class Sprint : Entity
{
    public string Name { get; private set; } = null!;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public SprintStatus Status { get; private set; }
    public string ProjectId { get; private set; } = null!;

    private Sprint() { }

    public Sprint(string id, string projectId, string name, DateTime start, DateTime end) : base(id)
    {
        ProjectId = Guard.NotNullOrWhiteSpace(projectId, nameof(projectId));
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        Guard.Against(end < start, "Sprint end date must be >= start date.");

        StartDate = start;
        EndDate = end;
        Status = SprintStatus.Planned;
    }

    public static Sprint Rehydrate(
        string id,
        DateTime createdAt,
        DateTime? updatedAt,
        string projectId,
        string name,
        DateTime start,
        DateTime end,
        SprintStatus status)
    {
        var s = new Sprint(id, projectId, name, start, end);
        s.SetTimestamps(createdAt, updatedAt);
        s.Status = status;
        return s;
    }

    public void Start()
    {
        Guard.Against(Status != SprintStatus.Planned, "Only planned sprint can be started.");
        Status = SprintStatus.Active;
        Touch();
    }

    public void Complete()
    {
        Guard.Against(Status != SprintStatus.Active, "Only active sprint can be completed.");
        Status = SprintStatus.Completed;
        Touch();
    }
}