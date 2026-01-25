using Domain.Enums;

namespace Domain.Models;

public class Team : Entity
{
    public string TenantId { get; private set; } = null!;
    public string TeamLeadId { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Department Department { get; private set; }
    public int? Capacity { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime UpdatedAt { get; private set; }

    public Team() { }

    public Team(string id, string tenantId, string teamLeadId, string name, Department department, bool isActive) : base(id)
    {
        TenantId = tenantId;
        TeamLeadId = teamLeadId;
        Name = name;
        Department = department;
        UpdatedAt = DateTime.UtcNow;
        IsActive = isActive;
    }

    public void UpdateDetails(string name, string? description, int? capacity)
    {
        Name = name;
        Description = description;
        Capacity = capacity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeLead(string newLeadId)
    {
        TeamLeadId = newLeadId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ToggleActivation()
    {
        IsActive = !IsActive;
        UpdatedAt = DateTime.UtcNow;
    }
}