namespace Domain.Models;

public class Project : Entity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string CreatedByUserId { get; private set; } = null!;
    public string TeamId { get; private set; } = null!;
    public string TenantId { get; private set; } = null!;

    public Project() { }

    public Project(string id, string name, string createdByUserId, string teamId, string tenantId) : base(id)
    {
        Name = name;
        CreatedByUserId = createdByUserId;
        TeamId = teamId;
        TenantId = tenantId;
    }

    public void Rename(string name, string? description = null)
    {
        Name = name;
        Description = description;
    }
}
