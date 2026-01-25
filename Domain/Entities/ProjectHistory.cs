namespace Domain.Models;

public sealed class ProjectHistory : Entity
{
    public string ProjectId { get; private set; } = null!;
    public string ChangedByUserId { get; private set; } = null!;
    public string? PropertyName { get; private set; }
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }

    public ProjectHistory() { }

    public ProjectHistory(
        string id,
        string projectId,
        string changedByUserId,
        string? propertyName,
        string? oldValue,
        string? newValue) : base(id)
    {
        ProjectId = projectId;
        ChangedByUserId = changedByUserId;
        PropertyName = propertyName;
        OldValue = oldValue;
        NewValue = newValue;
    }
}
