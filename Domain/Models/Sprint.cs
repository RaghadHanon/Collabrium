using Domain.Common;
using Domain.Enums;

namespace Domain.Models;

public class Sprint : Entity
{
    public string TenantId { get; private set; } = null!;
    public string ProjectId { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public string? Name { get; private set; } 
    public string? Goal { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public SprintStatus Status { get; private set; }

    private Sprint() { }

    public Sprint(string tenantId, string projectId, string code)
    {
        TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId));
        ProjectId = Guard.NotNullOrWhiteSpace(projectId, nameof(projectId));
        Code = Guard.NotNullOrWhiteSpace(code, nameof(code));

        Status = SprintStatus.Planned;
        Touch();
    }

    public Sprint(string id, string tenantId, string projectId, string code)
        : base(id)
    {
        TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId));
        ProjectId = Guard.NotNullOrWhiteSpace(projectId, nameof(projectId));
        Code = Guard.NotNullOrWhiteSpace(code, nameof(code));

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
        DateTime start,
        DateTime end,
        SprintStatus status)
    {
        var sprint = new Sprint(id, tenantId, projectId, code);
        sprint.Name = name;
        sprint.Goal = goal; 
        sprint.SetTimestamps(createdAt, updatedAt);
        sprint.Status = status; 
        sprint.StartDate = start;
        sprint.EndDate = end;

        return sprint;
    }

    public Sprint Clone()
    {
        return new Sprint(
            id: this.Id,
            tenantId: this.TenantId,
            projectId: this.ProjectId,
            code: this.Code
        )
        {
            Goal = this.Goal,
            Name = this.Name,
            StartDate = this.StartDate,
            EndDate = this.EndDate,
            CreatedAt = this.CreatedAt,
            UpdatedAt = this.UpdatedAt,
            Status = this.Status
        };
    }

    public void Update(DateTime start, DateTime end, string? name, string? goal)
    {
        Guard.Against(Status == SprintStatus.Completed, "Cannot update a completed sprint.");

        Schedule(start, end);

        Name = name;
        Goal = goal;

        Touch();
    }

    public void Start(DateTime start, DateTime end, string? name, string? goal)
    {
        Guard.Against(Status != SprintStatus.Planned, "Only planned sprint can be started.");

        Schedule(start, end);

        Status = SprintStatus.Active;

        Name = name;
        Goal = goal;

        Touch();
    }

    private void Schedule(DateTime start, DateTime end)
    {
        Guard.Against(
            Status == SprintStatus.Active && DateTime.UtcNow >= StartDate && start != StartDate,
            "Cannot update start date for a sprint that has already started."
        );
        Guard.Against(start <= DateTime.UtcNow, "Start date must be in the future.");
        Guard.Against(end < start, "Sprint end date must be >= start date.");

        StartDate = start;
        EndDate = end;
    }

    public void Complete()
    {
        Guard.Against(Status != SprintStatus.Active, "Only active sprint can be completed.");

        Status = SprintStatus.Completed;
        EndDate = DateTime.UtcNow;

        Touch();
    }

    public void Cancel()
    {
        Guard.Against(Status == SprintStatus.Completed, "Cannot cancel a completed sprint.");

        Status = SprintStatus.Cancelled;

        Touch();
    }
}