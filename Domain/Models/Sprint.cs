using Domain.Core;
using Domain.Enums;

namespace Domain.Models;

public class Sprint : Entity
{
    public string TenantId { get; private set; } = null!;
    public string ProjectId { get; private set; } = null!;
    public string Code { get; private set; } = null!;

    public string? Name { get; private set; }
    public string? Goal { get; private set; }

    // Planned dates
    public DateTime? PlannedStartDate { get; private set; }
    public DateTime? PlannedEndDate { get; private set; }

    // Actual dates
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public SprintStatus Status { get; private set; }

    public bool IsScheduled =>
        PlannedStartDate.HasValue &&
        PlannedEndDate.HasValue;

    private Sprint()
    {
    }

    public Sprint(
        string tenantId,
        string projectId,
        string code)
    {
        TenantId = Guard.NotNullOrWhiteSpace(
            tenantId,
            nameof(tenantId));

        ProjectId = Guard.NotNullOrWhiteSpace(
            projectId,
            nameof(projectId));

        Code = Guard.NotNullOrWhiteSpace(
            code,
            nameof(code));

        Status = SprintStatus.Planned;

        Touch();
    }

    public Sprint(
        string id,
        string tenantId,
        string projectId,
        string code)
        : base(id)
    {
        TenantId = Guard.NotNullOrWhiteSpace(
            tenantId,
            nameof(tenantId));

        ProjectId = Guard.NotNullOrWhiteSpace(
            projectId,
            nameof(projectId));

        Code = Guard.NotNullOrWhiteSpace(
            code,
            nameof(code));

        Status = SprintStatus.Planned;
    }

    public static Sprint Rehydrate(
        string id,
        DateTime createdAt,
        DateTime? updatedAt,
        string? name,
        string code,
        string? goal,
        string tenantId,
        string projectId,
        DateTime? plannedStartDate,
        DateTime? plannedEndDate,
        DateTime? startedAt,
        DateTime? completedAt,
        SprintStatus status)
    {
        var sprint = new Sprint(
            id,
            tenantId,
            projectId,
            code);

        sprint.Name = name;
        sprint.Goal = goal;

        sprint.PlannedStartDate = plannedStartDate;
        sprint.PlannedEndDate = plannedEndDate;

        sprint.StartedAt = startedAt;
        sprint.CompletedAt = completedAt;

        sprint.Status = status;

        sprint.SetTimestamps(
            createdAt,
            updatedAt);

        return sprint;
    }

    public Sprint Clone()
    {
        return new Sprint(
            id: Id,
            tenantId: TenantId,
            projectId: ProjectId,
            code: Code)
        {
            Name = Name,
            Goal = Goal,

            PlannedStartDate = PlannedStartDate,
            PlannedEndDate = PlannedEndDate,

            StartedAt = StartedAt,
            CompletedAt = CompletedAt,

            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt,

            Status = Status
        };
    }

    public void Update(
        DateTime? start,
        DateTime? end,
        string? name,
        string? goal)
    {
        Guard.Against(
            Status == SprintStatus.Completed,
            "Cannot update a completed sprint.");

        Schedule(start.Value, end.Value);

        Name = name;
        Goal = goal;

        Touch();
    }

    public void Start(
        string? name,
        string? goal)
    {
        Guard.Against(
            Status != SprintStatus.Planned,
            "Only a planned sprint can be started.");

        Status = SprintStatus.Active;
        StartedAt = DateTime.UtcNow;
        CompletedAt = null;

        Name = name;
        Goal = goal;

        Touch();
    }

    private void Schedule(
        DateTime start,
        DateTime end)
    {
        if (PlannedStartDate != start)
        {
            Guard.Against(
                Status == SprintStatus.Active &&
                StartedAt.HasValue,
                "Cannot update the planned start date after the sprint has started.");

            Guard.Against(
                start <= DateTime.UtcNow,
                "Planned start date must be in the future.");

            PlannedStartDate = start;
        }

        Guard.Against(
            end < start,
            "Planned end date must be greater than or equal to the planned start date.");

        if (PlannedEndDate != end)
            PlannedEndDate = end;
    }

    public void Complete()
    {
        Guard.Against(
            Status != SprintStatus.Active,
            "Only an active sprint can be completed.");

        Status = SprintStatus.Completed;
        CompletedAt = DateTime.UtcNow;

        Touch();
    }

    public void Cancel()
    {
        Guard.Against(
            Status == SprintStatus.Completed,
            "Cannot cancel a completed sprint.");

        Status = SprintStatus.Cancelled;

        Touch();
    }
}