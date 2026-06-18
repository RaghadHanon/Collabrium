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
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }

    // Actual dates
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public SprintStatus Status { get; private set; }

    public bool IsScheduled =>
        StartDate.HasValue &&
        EndDate.HasValue;

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
        DateTime? startDate,
        DateTime? endDate,
        DateTime? startedAt,
        DateTime? completedAt,
        SprintStatus status)
    {
        var sprint = new Sprint(
            id,
            tenantId,
            projectId,
            code)
        {
            Name = name,
            Goal = goal,

            StartDate = startDate,
            EndDate = endDate,

            StartedAt = startedAt,
            CompletedAt = completedAt,

            Status = status
        };

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

            StartDate = StartDate,
            EndDate = EndDate,

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
            Status is SprintStatus.Completed or SprintStatus.Cancelled,
            $"Cannot update a {Status} sprint.");

        var scheduleChanged = false;

        if (start.HasValue || end.HasValue)
        {
            scheduleChanged = ApplyScheduleChanges(
                start,
                end);
        }

        var normalizedName = name?.Trim();
        var normalizedGoal = goal?.Trim();

        var detailsChanged =
            !string.Equals(
                Name,
                normalizedName,
                StringComparison.Ordinal) ||
            !string.Equals(
                Goal,
                normalizedGoal,
                StringComparison.Ordinal);

        if (!scheduleChanged && !detailsChanged)
            return;

        Name = normalizedName;
        Goal = normalizedGoal;

        Touch();
    }

    public void UpdateSchedule(
        DateTime? start = null,
        DateTime? end = null)
    {
        Guard.Against(
            !start.HasValue && !end.HasValue,
            "At least one schedule date must be provided.");

        var changed = ApplyScheduleChanges(
            start,
            end);

        if (changed)
            Touch();
    }

    public void Start(
        DateTime start,
        DateTime end,
        string? name,
        string? goal)
    {
        Guard.Against(
            Status != SprintStatus.Planned,
            "Only a planned sprint can be started.");

        Guard.Against(
            StartedAt.HasValue,
            "Sprint has already been started.");

        var now = DateTime.UtcNow;

        ValidateScheduleForStarting(
            start,
            end,
            now);

        StartDate = start;
        EndDate = end;

        Name = name?.Trim();
        Goal = goal?.Trim();

        Status = SprintStatus.Active;
        StartedAt = now;
        CompletedAt = null;

        Touch();
    }

    public void Complete()
    {
        Guard.Against(
            Status != SprintStatus.Active,
            "Only an active sprint can be completed.");

        Guard.Against(
            !StartedAt.HasValue,
            "Sprint actual start time is missing.");

        Guard.Against(
            !IsScheduled,
            "Sprint planned schedule is missing.");

        var now = DateTime.UtcNow;

        Guard.Against(
            now < StartedAt.Value,
            "Sprint completion time cannot be before its actual start time.");

        Status = SprintStatus.Completed;
        CompletedAt = now;

        Touch();
    }

    public void Cancel()
    {
        Guard.Against(
            Status == SprintStatus.Completed,
            "Cannot cancel a completed sprint.");

        Guard.Against(
            Status == SprintStatus.Cancelled,
            "Sprint is already cancelled.");

        Status = SprintStatus.Cancelled;

        Touch();
    }

    private bool ApplyScheduleChanges(
        DateTime? start,
        DateTime? end)
    {
        Guard.Against(
            Status is SprintStatus.Completed or SprintStatus.Cancelled,
            $"Cannot update the schedule of a {Status} sprint.");

        var scheduleIsNotInitialized =
            !StartDate.HasValue ||
            !EndDate.HasValue;

        Guard.Against(
            scheduleIsNotInitialized &&
            (!start.HasValue || !end.HasValue),
            "Both start date and end date are required when scheduling the sprint for the first time.");

        var effectiveStartDate =
            start ?? StartDate!.Value;

        var effectiveEndDate =
            end ?? EndDate!.Value;

        var startDateChanged =
            start.HasValue &&
            start.Value != StartDate;

        var endDateChanged =
            end.HasValue &&
            end.Value != EndDate;

        if (!startDateChanged && !endDateChanged)
            return false;

        var now = DateTime.UtcNow;

        ValidatePlannedStartDate(
            effectiveStartDate,
            startDateChanged,
            now);

        ValidatePlannedEndDate(
            effectiveStartDate,
            effectiveEndDate,
            endDateChanged,
            now);

        StartDate = effectiveStartDate;
        EndDate = effectiveEndDate;

        return true;
    }

    private void ValidatePlannedStartDate(
        DateTime effectiveStartDate,
        bool startDateChanged,
        DateTime now)
    {
        if (!startDateChanged)
            return;

        Guard.Against(
            Status != SprintStatus.Planned,
            "The planned start date cannot be changed after the sprint has started.");

        Guard.Against(
            StartedAt.HasValue,
            "The planned start date cannot be changed because the sprint has already started.");

        Guard.Against(
            effectiveStartDate <= now,
            "The planned start date must be in the future.");
    }

    private void ValidatePlannedEndDate(
        DateTime effectiveStartDate,
        DateTime effectiveEndDate,
        bool endDateChanged,
        DateTime now)
    {
        Guard.Against(
            effectiveEndDate <= effectiveStartDate,
            "Sprint end date must be greater than the start date.");

        if (!endDateChanged)
            return;

        Guard.Against(
            Status == SprintStatus.Active &&
            effectiveEndDate <= now,
            "The planned end date of an active sprint must be in the future.");

        Guard.Against(
            StartedAt.HasValue &&
            effectiveEndDate <= StartedAt.Value,
            "The planned end date must be after the actual sprint start time.");
    }

    private static void ValidateScheduleForStarting(
        DateTime start,
        DateTime end,
        DateTime now)
    {
        Guard.Against(
            start == default,
            "Sprint planned start date is required.");

        Guard.Against(
            end == default,
            "Sprint planned end date is required.");

        Guard.Against(
            end <= start,
            "Sprint end date must be greater than the start date.");

        Guard.Against(
            start > now,
            "The sprint cannot be started before its planned start date.");

        Guard.Against(
            end <= now,
            "The sprint cannot be started because its planned end date has already passed.");
    }
}