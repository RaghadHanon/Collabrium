using Domain.Core;
using Domain.Enums;

namespace Domain.Models;

public class ProjectHistory : Entity
{
    public string ProjectId { get; private set; } = null!;
    public string RelatedEntityId { get; private set; } = null!;
    public string RelatedEntityCode { get; private set; } = null!;
    public string RelatedEntityTitle { get; private set; } = null!;
    public ProjectHistoryScope Scope { get; private set; }
    public ActionType ActionType { get; private set; }
    public string MadeByUserId { get; private set; } = null!;

    public string? PropertyName { get; private set; }
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }

    private ProjectHistory() { }

    public ProjectHistory(
        string id,
        string projectId,
        string relatedEntityId,
        string relatedEntityTitle,
        string relatedEntityCode,
        string madeByUserId,
        ActionType actionType,
        ProjectHistoryScope scope,
        string? propertyName,
        string? oldValue,
        string? newValue) : base(id)
    {
        ProjectId = Guard.NotNullOrWhiteSpace(projectId, nameof(projectId));
        RelatedEntityId = Guard.NotNullOrWhiteSpace(relatedEntityId, nameof(relatedEntityId));
        RelatedEntityTitle = Guard.NotNullOrWhiteSpace(relatedEntityTitle, nameof(relatedEntityTitle));
        RelatedEntityCode = Guard.NotNullOrWhiteSpace(relatedEntityCode, nameof(relatedEntityCode));
        MadeByUserId = Guard.NotNullOrWhiteSpace(madeByUserId, nameof(madeByUserId));
        Scope = scope;
        ActionType = actionType;
        PropertyName = propertyName;

        OldValue = oldValue;
        NewValue = newValue;
    }

    public ProjectHistory(
      string projectId,
      string relatedEntityId,
      string relatedEntityTitle,
      string relatedEntityCode,
      string madeByUserId,
      ProjectHistoryScope scope,
      ActionType actionType,
      string? propertyName,
      string? oldValue,
      string? newValue)
    {
        ProjectId = Guard.NotNullOrWhiteSpace(projectId, nameof(projectId));
        RelatedEntityId = Guard.NotNullOrWhiteSpace(relatedEntityId, nameof(relatedEntityId));
        RelatedEntityTitle = Guard.NotNullOrWhiteSpace(relatedEntityTitle, nameof(relatedEntityTitle));
        RelatedEntityCode = Guard.NotNullOrWhiteSpace(relatedEntityCode, nameof(relatedEntityCode));
        MadeByUserId = Guard.NotNullOrWhiteSpace(madeByUserId, nameof(madeByUserId));
        Scope = scope;
        ActionType = actionType;
        PropertyName = propertyName;

        OldValue = oldValue;
        NewValue = newValue;
    }

    public static ProjectHistory Rehydrate(
    string id,
    DateTime createdAt,
    DateTime? updatedAt,
    string projectId,
    string relatedEntityId,
    string relatedEntityTitle,
    string relatedEntityCode,
    string madeByUserId,
    ProjectHistoryScope scope,
    ActionType actionType,
    string? propertyName,
    string? oldValue,
    string? newValue)
    {
        var history = new ProjectHistory(
            id,
            projectId,
            relatedEntityId,
            relatedEntityTitle,
            relatedEntityCode,
            madeByUserId,
            actionType,
            scope,
            propertyName,
            oldValue,
            newValue);

        history.SetTimestamps(createdAt, updatedAt);
        return history;
    }
}