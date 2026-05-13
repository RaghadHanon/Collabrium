using Domain.Core;
using Domain.Enums;

namespace Domain.Models;

public sealed class Team : Entity
{
    public string TenantId { get; private set; } = null!;
    public string TeamLeadId { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Department Department { get; private set; }
    public int? Capacity { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly HashSet<string> _memberIds = new();
    public IReadOnlyCollection<string> MemberIds => _memberIds;

    private Team() { }

    public Team(string tenantId, string teamLeadId, string name, Department department,
        string? description, int? capacity = null, bool isActive = true)
    {
        TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId));
        TeamLeadId = Guard.NotNullOrWhiteSpace(teamLeadId, nameof(teamLeadId));
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        Guard.Against(capacity is < 1, "Capacity must be >= 1 if provided.");

        Department = department;
        Capacity = capacity;
        Description = description?.Trim();
        IsActive = isActive;

        _memberIds.Add(TeamLeadId);
        Touch();
    }

    public Team(string id, string tenantId, string teamLeadId, string name,
        Department department, string? description, int? capacity = null, bool isActive = true)
        : this(tenantId, teamLeadId, name, department, description, capacity, isActive)
    {
        Id = id;
    }

    public static Team Rehydrate(
        string id,
        DateTime createdAt,
        DateTime? updatedAt,
        string tenantId,
        string teamLeadId,
        string name,
        string? description,
        Department department,
        int? capacity,
        bool isActive,
        IEnumerable<string> memberIds)
    {
        var t = new Team(id, tenantId, teamLeadId, name, department, description, capacity, isActive);
        t.SetTimestamps(createdAt, updatedAt);

        t.Description = description?.Trim();
        t.Capacity = capacity;
        t.IsActive = isActive;

        t._memberIds.Clear();
        foreach (var m in memberIds.Distinct())
            t._memberIds.Add(m);

        t._memberIds.Add(teamLeadId);

        return t;
    }

    public void UpdateDetails(string name, string? description, int? capacity, Department? department)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        Description = description?.Trim();
        Guard.Against(capacity is < 1, "Capacity must be >= 1 if provided.");
        Capacity = capacity;
        if (department.HasValue)
            Department = department.Value;
        Touch();
    }

    public void ChangeLead(string? newLeadId)
    {
        newLeadId = Guard.NotNullOrWhiteSpace(newLeadId, nameof(newLeadId));
        _memberIds.Add(newLeadId);
        TeamLeadId = newLeadId;
        Touch();
    }

    public void AddMember(string userId)
    {
        Guard.Against(!IsActive, "Cannot add members to an inactive team.");
        userId = Guard.NotNullOrWhiteSpace(userId, nameof(userId));

        if (Capacity is not null)
            Guard.Against(_memberIds.Count >= Capacity.Value, "Team capacity reached.");

        Guard.Against(!_memberIds.Add(userId), "User is already a team member.");
        Touch();
    }

    public void RemoveMember(string userId)
    {
        userId = Guard.NotNullOrWhiteSpace(userId, nameof(userId));
        Guard.Against(userId == TeamLeadId, "Cannot remove the team lead.");
        Guard.Against(!_memberIds.Remove(userId), "User is not a team member.");
        Touch();
    }

    public void Activate() { IsActive = true; Touch(); }
    public void Deactivate() { IsActive = false; Touch(); }
}