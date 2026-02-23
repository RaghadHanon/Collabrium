using Domain.Common;
using Domain.Enums;

namespace Domain.Models;

public sealed class Sprint : Entity
{
    public string TenantId { get; private set; } = null!;
    public string ProjectId { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Goal { get; private set; } = null!;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public SprintStatus Status { get; private set; }

    private Sprint() { }

    public Sprint(string tenantId, string projectId, string name, string goal, DateTime start, DateTime end)
    {
        TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId));
        ProjectId = Guard.NotNullOrWhiteSpace(projectId, nameof(projectId));
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        Goal = Guard.NotNullOrWhiteSpace(goal, nameof(goal));

        Guard.Against(end < start, "Sprint end date must be >= start date.");

        StartDate = start;
        EndDate = end;
        Status = SprintStatus.Planned;

        Touch();
    }

    public Sprint(string id, string tenantId, string projectId, string name, string goal, DateTime start, DateTime end)
        : base(id)
    {
        TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId));
        ProjectId = Guard.NotNullOrWhiteSpace(projectId, nameof(projectId));
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        Goal = Guard.NotNullOrWhiteSpace(goal, nameof(goal));

        Guard.Against(end < start, "Sprint end date must be >= start date.");

        StartDate = start;
        EndDate = end;
        Status = SprintStatus.Planned;
    }

    public static Sprint Rehydrate(
        string id,
        DateTime createdAt,
        DateTime? updatedAt,
        string tenantId,
        string projectId,
        string name,
        string goal,
        DateTime start,
        DateTime end,
        SprintStatus status)
    {
        var sprint = new Sprint(id, tenantId, projectId, name, goal, start, end);
        sprint.SetTimestamps(createdAt, updatedAt);
        sprint.Status = status;
        return sprint;
    }

    public void UpdateDetails(string name, string goal, DateTime start, DateTime end)
    {
        Guard.Against(Status == SprintStatus.Completed, "Cannot update a completed sprint.");
        Guard.Against(end < start, "Sprint end date must be >= start date.");
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        Goal = Guard.NotNullOrWhiteSpace(goal, nameof(goal));
        StartDate = start;
        EndDate = end;
        Touch();
    }

    public void Start()
    {
        Guard.Against(Status != SprintStatus.Planned && Status != SprintStatus.Active, "Only planned sprint can be started.");
        Status = SprintStatus.Active;
        Touch();
    }

    public void Complete()
    {
        Guard.Against(Status != SprintStatus.Active && Status != SprintStatus.Completed, "Only active sprint can be completed.");
        Status = SprintStatus.Completed;
        Touch();
    }

    public void Cancel()
    {
        Guard.Against(Status == SprintStatus.Completed, "Cannot cancel a completed sprint.");
        Status = SprintStatus.Cancelled;
        Touch();
    }
}