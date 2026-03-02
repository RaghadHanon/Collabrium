using Domain.Common;
using Domain.Enums;

namespace Domain.Models;

public class Sprint : Entity
{
    public string TenantId { get; private set; } = null!;
    public string ProjectId { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public SprintStatus Status { get; private set; }

    private Sprint() { }

    public Sprint(string tenantId, string projectId, string name)
    {
        TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId));
        ProjectId = Guard.NotNullOrWhiteSpace(projectId, nameof(projectId));
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));

        Status = SprintStatus.Planned;
        Touch();
    }

    public Sprint(string id, string tenantId, string projectId, string name)
        : base(id)
    {
        TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId));
        ProjectId = Guard.NotNullOrWhiteSpace(projectId, nameof(projectId));
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));

        Status = SprintStatus.Planned;
    }

    public static Sprint Rehydrate(
        string id,
        DateTime createdAt,
        DateTime? updatedAt,
        string name,
        string tenantId,
        string projectId,
        DateTime start,
        DateTime end,
        SprintStatus status)
    {
        var sprint = new Sprint(id, tenantId, projectId, name);
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
            name: this.Name
        )
        {
            StartDate = this.StartDate,
            EndDate = this.EndDate,
            CreatedAt = this.CreatedAt,
            UpdatedAt = this.UpdatedAt,
            Status = this.Status
        };
    }

    public void Schedule(DateTime start, DateTime end)
    {
        Guard.Against(Status == SprintStatus.Completed, "Cannot update a completed sprint.");
        Guard.Against(DateTime.UtcNow > StartDate && Status == SprintStatus.Active, "Cannot update start date for a sprint that has already started.");
        Guard.Against(end < start, "Sprint end date must be >= start date.");
        StartDate = start;
        EndDate = end;
        Touch();
    }

    public void Start(DateTime start, DateTime end)
    {
        Guard.Against(Status != SprintStatus.Planned && Status != SprintStatus.Active, "Only planned sprint can be started.");
        Guard.Against(DateTime.UtcNow < StartDate, "Start date must be in the future.");
        Guard.Against(end < start, "Sprint end date must be >= start date.");
        Status = SprintStatus.Active;
        StartDate = start;
        EndDate = end;
        Touch();
    }

    public void Complete()
    {
        Guard.Against(Status != SprintStatus.Active && Status != SprintStatus.Completed, "Only active sprint can be completed.");
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