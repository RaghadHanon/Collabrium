using Domain.Core;
using Domain.Enums;
using System.Text.RegularExpressions;

namespace Domain.Models;

public sealed class Project : Entity
{
    public string Name { get; private set; } = null!;
    public string Key { get; private set; } = null!;
    public string? Description { get; private set; }

    public ProjectStatus Status { get; private set; } = ProjectStatus.NotStarted;

    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }

    public int LastWorkItemNumber { get; set; } = 0;
    public int LastSprintNumber { get; set; } = 0;

    public string CreatedByUserId { get; private set; } = null!;
    public string? TeamId { get; private set; }
    public string TenantId { get; private set; } = null!;

    private Project() { }

    public Project(
        string name,
        string key,
        string createdByUserId,
        string tenantId,
        string? teamId = null,
        string? description = null,
        ProjectStatus status = ProjectStatus.NotStarted,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));

        Guard.Against(!Regex.IsMatch(key, "^[A-Z]{2,10}$"), "Project key must be 2-10 uppercase letters only.");
        Key = Guard.NotNullOrWhiteSpace(key, nameof(key));

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
        string key,
        int lastWorkItemNumber,
        int lastSprintNumber,
        string createdByUserId,
        string tenantId,
        string? teamId = null)
        : base(id)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));

        Guard.Against(!Regex.IsMatch(key, "^[A-Z]{2,10}$"), "Project key must be 2-10 uppercase letters only.");
        Key = Guard.NotNullOrWhiteSpace(key, nameof(key));

        LastWorkItemNumber = lastWorkItemNumber;
        LastSprintNumber = lastSprintNumber;
        CreatedByUserId = Guard.NotNullOrWhiteSpace(createdByUserId, nameof(createdByUserId));
        TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId));
        TeamId = string.IsNullOrWhiteSpace(teamId) ? null : teamId.Trim();
    }

    public static Project Rehydrate(
        string id,
        DateTime createdAt,
        DateTime? updatedAt,
        string name,
        string key,
        int lastWorkItemNumber,
        int lastSprintNumber,
        string? description,
        ProjectStatus status,
        DateTime? startDate,
        DateTime? endDate,
        string createdByUserId,
        string? teamId,
        string tenantId)
    {
        var p = new Project(id, name, key, lastWorkItemNumber, lastSprintNumber, createdByUserId, tenantId, teamId);

        p.SetTimestamps(createdAt, updatedAt);

        p.Description = description?.Trim();
        p.SetSchedule(startDate, endDate);
        p.SetStatus(status);

        return p;
    }

    public Project Clone()
    {
        return new Project(
            id: this.Id,
            name: this.Name,
            key: this.Key,
            lastWorkItemNumber: this.LastWorkItemNumber,
            lastSprintNumber: this.LastSprintNumber,
            createdByUserId: this.CreatedByUserId,
            tenantId: this.TenantId,
            teamId: this.TeamId
        )
        {
            CreatedAt = this.CreatedAt,
            UpdatedAt = this.UpdatedAt,
            Description = this.Description,
            Status = this.Status,
            StartDate = this.StartDate,
            EndDate = this.EndDate,
            CreatedByUserId = this.CreatedByUserId
        };
    }

    public string GenerateNextWorkItemCode()
    {
        LastWorkItemNumber++;
        return $"{Key}-{LastWorkItemNumber}";
    }

    public string GenerateNextSprintName()
    {
        LastSprintNumber++;
        return $"{Key} Sprint {LastSprintNumber}";
    }

    public void UpdateDetails(string name, string? description,
        DateTime? startDate, DateTime? endDate)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        Description = description?.Trim();
        SetSchedule(startDate, endDate);
        Touch();
    }

    public void ChangeStatusWithValidation(ProjectStatus status,
         IEnumerable<WorkItemSnapshot> workItemSnapshots)
    {
        if (status == ProjectStatus.Completed)
            Guard.Against(workItemSnapshots.Any(wi => wi.Status != WorkItemStatus.Done),
                "Cannot mark project as Completed if it has incomplete work items.");

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
            Guard.Against(endDate.Value < startDate.Value, "PlannedEndDate cannot be before PlannedStartDate.");

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