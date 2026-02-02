using Domain.Common;

namespace Domain.Models;

public sealed class ProjectHistory : Entity
{
    public string ProjectId { get; private set; } = null!;
    public string ChangedByUserId { get; private set; } = null!;
    public string? PropertyName { get; private set; }
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }

    private ProjectHistory() { } // EF

    public ProjectHistory(
        string id,
        string projectId,
        string changedByUserId,
        string? propertyName,
        string? oldValue,
        string? newValue) : base(id)
    {
        ProjectId = Guard.NotNullOrWhiteSpace(projectId, nameof(projectId));
        ChangedByUserId = Guard.NotNullOrWhiteSpace(changedByUserId, nameof(changedByUserId));
        PropertyName = propertyName?.Trim();
        OldValue = oldValue;
        NewValue = newValue;
    }

    public static ProjectHistory Rehydrate(
    string id,
    DateTime createdAt,
    DateTime? updatedAt,
    string projectId,
    string changedByUserId,
    string? propertyName,
    string? oldValue,
    string? newValue)
    {
        var history = new ProjectHistory(
            id,
            projectId,
            changedByUserId,
            propertyName,
            oldValue,
            newValue);

        history.SetTimestamps(createdAt, updatedAt); // من Entity base
        return history;
    }
}