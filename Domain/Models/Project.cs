using Domain.Common;

namespace Domain.Entities;

public sealed class Project : Entity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }

    public string CreatedByUserId { get; private set; } = null!;
    public string TeamId { get; private set; } = null!;
    public string TenantId { get; private set; } = null!;

    private Project() { }

    public Project(string id, string name, string createdByUserId, string teamId, string tenantId) : base(id)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        CreatedByUserId = Guard.NotNullOrWhiteSpace(createdByUserId, nameof(createdByUserId));
        TeamId = Guard.NotNullOrWhiteSpace(teamId, nameof(teamId));
        TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId));
    }

    public static Project Rehydrate(
        string id,
        DateTime createdAt,
        DateTime? updatedAt,
        string name,
        string? description,
        string createdByUserId,
        string teamId,
        string tenantId)
    {
        var p = new Project(id, name, createdByUserId, teamId, tenantId);
        p.SetTimestamps(createdAt, updatedAt);
        p.Description = description?.Trim();
        return p;
    }

    public void Rename(string name, string? description = null)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        Description = description?.Trim();
        Touch();
    }

    public void ChangeTeam(string teamId)
    {
        TeamId = Guard.NotNullOrWhiteSpace(teamId, nameof(teamId));
        Touch();
    }
}