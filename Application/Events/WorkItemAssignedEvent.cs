using MediatR;

namespace Application.Events;

public record WorkItemAssignedEvent(
    string TenantId,
    string ProjectId,
    string WorkItemId,
    string WorkItemTitle,
    string WorkItemCode,
    string WorkItemType,
    string WorkItemPriority,
    string? SprintId,
    string AssigneeUserId,
    string AssigneeEmail,
    string AssigneeFullName,
    string AssignerUserId,
    string AssignerFullName
) : INotification;