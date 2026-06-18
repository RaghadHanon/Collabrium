using Application.Events;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;

namespace Application.EventsHandlers;

public sealed class WorkItemDetailsChangedNotificationHandler(
INotificationCreationService notificationCreationService
) : INotificationHandler<WorkItemDetailsChangedEvent>
{
    public async Task Handle(
    WorkItemDetailsChangedEvent notification,
    CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(notification.TenantId) ||
        string.IsNullOrWhiteSpace(notification.AssignedToUserId) ||
        notification.Changes.Count == 0)
        {
            return;
        }

        if (string.Equals(
            notification.AssignedToUserId,
            notification.ChangedByUserId,
            StringComparison.Ordinal))
        {
            return;
        }

        var actionUrl = string.IsNullOrWhiteSpace(notification.SprintId)
            ? $"/projects/{notification.ProjectId}/backlog"
            : $"/projects/{notification.ProjectId}/board";

        foreach (var change in notification.Changes)
        {
            var propertyDisplayName =
                GetPropertyDisplayName(change.PropertyName);

            var message = BuildMessage(
                changedByFullName: notification.ChangedByFullName,
                propertyDisplayName: propertyDisplayName,
                oldValue: change.OldValue,
                newValue: change.NewValue,
                workItemCode: notification.WorkItemCode,
                workItemTitle: notification.WorkItemTitle);

            await notificationCreationService.CreateForUsersAsync(
                tenantId: notification.TenantId,
                recipientUserIds: [notification.AssignedToUserId],
                type: NotificationType.WorkItemUpdated,
                title: $"{propertyDisplayName} Updated",
                message: message,
                triggeredByUserId: notification.ChangedByUserId,
                entityType: "WorkItem",
                entityId: notification.WorkItemId,
                actionUrl: actionUrl,
                cancellationToken: cancellationToken);
        }
    }

    private static string BuildMessage(
        string changedByFullName,
        string propertyDisplayName,
        string? oldValue,
        string? newValue,
        string workItemCode,
        string workItemTitle)
    {
        var formattedOldValue = FormatValue(oldValue);
        var formattedNewValue = FormatValue(newValue);

        if (string.IsNullOrWhiteSpace(oldValue))
        {
            return
                $"{changedByFullName} set {propertyDisplayName} to " +
                $"'{formattedNewValue}' in {workItemCode} - {workItemTitle}.";
        }

        if (string.IsNullOrWhiteSpace(newValue))
        {
            return
                $"{changedByFullName} cleared {propertyDisplayName} " +
                $"in {workItemCode} - {workItemTitle}.";
        }

        return
            $"{changedByFullName} changed {propertyDisplayName} " +
            $"from '{formattedOldValue}' to '{formattedNewValue}' " +
            $"in {workItemCode} - {workItemTitle}.";
    }

    private static string FormatValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Not set";

        const int maximumLength = 100;

        return value.Length <= maximumLength
            ? value
            : $"{value[..maximumLength]}...";
    }

    private static string GetPropertyDisplayName(string propertyName)
    {
        return propertyName switch
        {
            "Title" => "Title",
            "Description" => "Description",
            "AcceptanceCriteria" => "Acceptance Criteria",
            "EffortPoints" => "Effort Points",
            "Priority" => "Priority",
            "SprintId" => "Sprint",
            "IsBlocked" => "Blocked State",
            "AssignedToUserId" => "Assignee",
            "ParentId" => "Parent Work Item",

            "StoryPoints" => "Story Points",

            "StepsToReproduce" => "Steps to Reproduce",
            "ExpectedResult" => "Expected Result",
            "ActualResult" => "Actual Result",

            "Preconditions" => "Preconditions",
            "Steps" => "Test Steps",
            "TestEnvironment" => "Test Environment",
            "Result" => "Test Result",

            _ => propertyName
        };
    }
}