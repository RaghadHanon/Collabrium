using Domain.Common;
using Domain.Enums;

namespace Domain.Models;

public sealed class Project : Entity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }

    public ProjectStatus Status { get; private set; } = ProjectStatus.NotStarted;

    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }

    public string CreatedByUserId { get; private set; } = null!;
    public string? TeamId { get; private set; }
    public string TenantId { get; private set; } = null!;

    private Project() { }

    public Project(
        string name,
        string createdByUserId,
        string tenantId,
        string? teamId = null,
        string? description = null,
        ProjectStatus status = ProjectStatus.NotStarted,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        CreatedByUserId = Guard.NotNullOrWhiteSpace(createdByUserId, nameof(createdByUserId));
        TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId));

        TeamId = string.IsNullOrWhiteSpace(teamId) ? null : teamId.Trim();
        Description = description?.Trim();

        SetSchedule(startDate, endDate);
        SetStatus(status);

        Touch();
    }

    public Project(
        string id,
        string name,
        string createdByUserId,
        string tenantId,
        string? teamId = null)
        : base(id)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        CreatedByUserId = Guard.NotNullOrWhiteSpace(createdByUserId, nameof(createdByUserId));
        TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId));
        TeamId = string.IsNullOrWhiteSpace(teamId) ? null : teamId.Trim();
    }

    public static Project Rehydrate(
        string id,
        DateTime createdAt,
        DateTime? updatedAt,
        string name,
        string? description,
        ProjectStatus status,
        DateTime? startDate,
        DateTime? endDate,
        string createdByUserId,
        string? teamId,
        string tenantId)
    {
        var p = new Project(id, name, createdByUserId, tenantId, teamId);

        p.SetTimestamps(createdAt, updatedAt);

        p.Description = description?.Trim();
        p.SetSchedule(startDate, endDate);
        p.SetStatus(status);

        return p;
    }

    public void UpdateDetails(string name, string? description, ProjectStatus? status,
        DateTime? startDate, DateTime? endDate)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        Description = description?.Trim();
        if (status.HasValue)
            SetStatus(status.Value);
        SetSchedule(startDate, endDate);
        Touch();
    }

    public void ChangeStatus(ProjectStatus status)
    {
        SetStatus(status);
        Touch();
    }

    public void Schedule(DateTime? startDate, DateTime? endDate)
    {
        SetSchedule(startDate, endDate);
        Touch();
    }

    public void AssignToTeam(string teamId)
    {
        TeamId = Guard.NotNullOrWhiteSpace(teamId, nameof(teamId)).Trim();
        Touch();
    }

    public void UnassignFromTeam()
    {
        TeamId = null;
        Touch();
    }

    private void SetSchedule(DateTime? startDate, DateTime? endDate)
    {
        if (startDate.HasValue && endDate.HasValue)
            Guard.Against(endDate.Value < startDate.Value, "EndDate cannot be before StartDate.");

        StartDate = startDate;
        EndDate = endDate;
    }

    private void SetStatus(ProjectStatus status)
    {
        if (status == ProjectStatus.Completed && EndDate is null)
            EndDate = DateTime.UtcNow;

        Status = status;
    }
}